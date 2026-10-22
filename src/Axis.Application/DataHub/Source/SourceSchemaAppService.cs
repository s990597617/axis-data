using Axis.Application.DataHub.Common;
using Axis.Domain.DataHub;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using SqlSugar;

namespace Axis.Application.DataHub.Source;

/// <summary>
/// 数据源元数据浏览应用服务：按连接配置运行时连接读取表/视图/列。
/// </summary>
public sealed class SourceSchemaAppService(
    IRepository<DhSource> repository,
    SourceConnectionFactory connectionFactory)
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
            throw new BusinessException("HTTP 数据源请使用「查询」对象类型并填写 SQL，无法浏览库表");

        try
        {
            var connectionString = connectionFactory.ResolveConnectionString(source);
            var client = connectionFactory.OpenClient(source.SourceType, connectionString);
            return (client, source);
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw WrapMetaException(ex);
        }
    }

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
            || combined.Contains("MySql.Data", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            return new BusinessException("当前环境未安装对应数据库驱动，无法读取字段元数据");
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