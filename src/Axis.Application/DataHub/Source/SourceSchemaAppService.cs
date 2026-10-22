using Axis.Application.DataHub.Common;
using Axis.Domain.DataHub;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using Microsoft.Extensions.Configuration;
using SqlSugar;

namespace Axis.Application.DataHub.Source;

/// <summary>
/// 数据源元数据浏览应用服务：按 SecretRef 打开临时连接读取表/视图/列。
/// </summary>
public sealed class SourceSchemaAppService(
    IRepository<DhSource> repository,
    IConfiguration configuration)
    : ISourceSchemaAppService, IAutoInject
{
    public async Task<IReadOnlyList<DataHubLookupItemDto>> ListObjectsAsync(
        long sourceId,
        DhObjectTypeEnum objectType,
        CancellationToken ct = default)
    {
        if (objectType == DhObjectTypeEnum.Query)
            return Array.Empty<DataHubLookupItemDto>();

        var (client, _) = await OpenClientAsync(sourceId, ct);
        try
        {
            return await Task.Run(() =>
            {
                try
                {
                    var list = objectType == DhObjectTypeEnum.View
                        ? client.DbMaintenance.GetViewInfoList(false)
                        : client.DbMaintenance.GetTableInfoList(false);

                    return list
                        .Select(item =>
                        {
                            var name = FormatObjectName(item);
                            return new DataHubLookupItemDto { Value = name, Label = name };
                        })
                        .OrderBy(item => item.Label, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
                catch (Exception ex) when (ex is not BusinessException)
                {
                    throw WrapMetaException(ex);
                }
            }, ct);
        }
        finally
        {
            client.Dispose();
        }
    }

    public async Task<IReadOnlyList<DataHubLookupItemDto>> ListColumnsAsync(
        long sourceId,
        string objectName,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            return Array.Empty<DataHubLookupItemDto>();

        var (client, _) = await OpenClientAsync(sourceId, ct);
        try
        {
            var name = objectName.Trim();
            return await Task.Run(() =>
            {
                try
                {
                    var columns = client.DbMaintenance.GetColumnInfosByTableName(name, false);
                    return columns
                        .Select(col =>
                        {
                            var colName = col.DbColumnName;
                            var type = col.DataType;
                            var label = string.IsNullOrWhiteSpace(type)
                                ? colName
                                : $"{colName}（{type}）";
                            return new DataHubLookupItemDto { Value = colName, Label = label };
                        })
                        .OrderBy(item => item.Value, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                }
                catch (Exception ex) when (ex is not BusinessException)
                {
                    throw WrapMetaException(ex);
                }
            }, ct);
        }
        finally
        {
            client.Dispose();
        }
    }

    private async Task<(SqlSugarClient Client, DhSource Source)> OpenClientAsync(long sourceId, CancellationToken ct)
    {
        var source = await repository.AsQueryable().Where(item => item.Id == sourceId).FirstAsync(ct)
                     ?? throw new BusinessException("数据源不存在");

        if (source.SourceType == DhSourceTypeEnum.Http)
            throw new BusinessException("HTTP 数据源请使用「查询」类型并手写 SQL，无法浏览对象");

        var connectionString = ResolveConnectionString(source.SecretRef);
        var dbType = MapDbType(source.SourceType);

        try
        {
            var client = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = connectionString,
                DbType = dbType,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
            return (client, source);
        }
        catch (Exception ex)
        {
            throw WrapMetaException(ex);
        }
    }

    private string ResolveConnectionString(string? secretRef)
    {
        if (string.IsNullOrWhiteSpace(secretRef))
            throw new BusinessException("数据源未配置连接或密钥引用无效");

        var raw = secretRef.Trim();
        const string configPrefix = "config:";
        const string envPrefix = "env:";

        if (raw.StartsWith(configPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var key = raw[configPrefix.Length..].Trim();
            if (string.IsNullOrWhiteSpace(key))
                throw new BusinessException("数据源未配置连接或密钥引用无效");

            var value = configuration[key];
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessException("数据源未配置连接或密钥引用无效");
            return value;
        }

        if (raw.StartsWith(envPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var name = raw[envPrefix.Length..].Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessException("数据源未配置连接或密钥引用无效");

            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessException("数据源未配置连接或密钥引用无效");
            return value;
        }

        throw new BusinessException("数据源未配置连接或密钥引用无效");
    }

    private static DbType MapDbType(DhSourceTypeEnum sourceType) => sourceType switch
    {
        DhSourceTypeEnum.SqlServer => DbType.SqlServer,
        DhSourceTypeEnum.PostgreSql => DbType.PostgreSQL,
        DhSourceTypeEnum.MySql => DbType.MySql,
        _ => throw new BusinessException("HTTP 数据源请使用「查询」类型并手写 SQL，无法浏览对象")
    };

    private static string FormatObjectName(DbTableInfo info)
    {
        var schema = info.GetType().GetProperty("Schema")?.GetValue(info) as string
                     ?? info.GetType().GetProperty("SchemaName")?.GetValue(info) as string;
        if (!string.IsNullOrWhiteSpace(schema))
            return $"{schema}.{info.Name}";
        return info.Name;
    }

    private static BusinessException WrapMetaException(Exception ex)
    {
        var message = ex.Message ?? string.Empty;
        var combined = $"{ex.GetType().Name} {message}";

        if (combined.Contains("Could not load file or assembly", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("未找到程序集", StringComparison.Ordinal)
            || combined.Contains("SqlClient", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("MySqlConnector", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("MySql.Data", StringComparison.OrdinalIgnoreCase))
        {
            return new BusinessException("当前环境未安装该数据库驱动，可手动输入对象名");
        }

        if (combined.Contains("connect", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("连接", StringComparison.Ordinal)
            || combined.Contains("timeout", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("network", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("Authentication", StringComparison.OrdinalIgnoreCase))
        {
            return new BusinessException($"无法连接数据源：{TrimMessage(message)}");
        }

        return new BusinessException($"读取元数据失败：{TrimMessage(message)}");
    }

    private static string TrimMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return "未知错误";
        message = message.ReplaceLineEndings(" ").Trim();
        return message.Length > 200 ? message[..200] + "…" : message;
    }
}