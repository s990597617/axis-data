using System.Diagnostics;
using Axis.Application.DataHub.Source.Dto;
using Axis.Domain.DataHub;
using Axis.Domain.Enums;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using Microsoft.Extensions.Configuration;
using SqlSugar;

namespace Axis.Application.DataHub.Source;

/// <summary>
/// 数据源连接解析与 SqlSugar / HTTP 连通能力（供元数据浏览与连通性测试复用）。
/// </summary>
public sealed class SourceConnectionFactory(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory)
    : IAutoInject
{
    /// <summary>
    /// 解析实体可用的连接串：优先内联 ConnectionString，其次 SecretRef。
    /// </summary>
    public string ResolveConnectionString(DhSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return ResolveConnectionString(source.SecretRef, source.ConnectionString);
    }

    /// <summary>
    /// 解析连接串：优先 connectionString，否则解析 SecretRef（config: / env:）。
    /// </summary>
    public string ResolveConnectionString(string? secretRef, string? connectionString)
    {
        if (!string.IsNullOrWhiteSpace(connectionString))
            return connectionString.Trim();

        return ResolveSecretRef(secretRef);
    }

    /// <summary>
    /// 由测试入参得到最终连接串（完整串 → 结构化拼装 → SecretRef）。
    /// </summary>
    public string ResolveForTest(TestSourceConnectionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!string.IsNullOrWhiteSpace(input.ConnectionString))
            return input.ConnectionString.Trim();

        var composed = ComposeFromBuilder(input);
        if (!string.IsNullOrWhiteSpace(composed))
            return composed;

        return ResolveSecretRef(input.SecretRef);
    }

    /// <summary>
    /// 打开 SQL 类数据源客户端；HTTP 类型请走 <see cref="TestHttpAsync"/>。
    /// </summary>
    public SqlSugarClient OpenClient(DhSourceTypeEnum sourceType, string connectionString)
    {
        if (sourceType == DhSourceTypeEnum.Http)
            throw new BusinessException("HTTP 数据源请使用「查询」对象类型并填写 SQL，无法打开库连接");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new BusinessException("数据源未配置连接串或密钥引用无效");

        return new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = MapDbType(sourceType),
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute
        });
    }

    /// <summary>
    /// 映射 SqlSugar DbType。
    /// </summary>
    public static DbType MapDbType(DhSourceTypeEnum sourceType) => sourceType switch
    {
        DhSourceTypeEnum.SqlServer => DbType.SqlServer,
        DhSourceTypeEnum.PostgreSql => DbType.PostgreSQL,
        DhSourceTypeEnum.MySql => DbType.MySql,
        _ => throw new BusinessException("HTTP 数据源请使用「查询」对象类型并填写 SQL，无法打开库连接")
    };

    /// <summary>
    /// 执行连通性测试（不落库）。
    /// </summary>
    public async Task<TestSourceConnectionResultDto> TestAsync(
        TestSourceConnectionInput input,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        var sw = Stopwatch.StartNew();

        try
        {
            if (input.SourceType == DhSourceTypeEnum.Http)
            {
                var baseUrl = !string.IsNullOrWhiteSpace(input.BaseUrl)
                    ? input.BaseUrl.Trim()
                    : (!string.IsNullOrWhiteSpace(input.ConnectionString)
                        ? input.ConnectionString.Trim()
                        : ResolveSecretRef(input.SecretRef));

                var httpResult = await TestHttpAsync(baseUrl, ct);
                sw.Stop();
                httpResult.ElapsedMs = (int)sw.ElapsedMilliseconds;
                return httpResult;
            }

            var connectionString = ResolveForTest(input);
            await TestSqlAsync(input.SourceType, connectionString, ct);
            sw.Stop();
            return new TestSourceConnectionResultDto
            {
                Success = true,
                Message = "连接成功",
                ElapsedMs = (int)sw.ElapsedMilliseconds
            };
        }
        catch (BusinessException ex)
        {
            sw.Stop();
            return new TestSourceConnectionResultDto
            {
                Success = false,
                Message = ex.Message,
                ElapsedMs = (int)sw.ElapsedMilliseconds
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new TestSourceConnectionResultDto
            {
                Success = false,
                Message = FormatTestFailure(ex),
                ElapsedMs = (int)sw.ElapsedMilliseconds
            };
        }
    }

    private async Task TestSqlAsync(DhSourceTypeEnum sourceType, string connectionString, CancellationToken ct)
    {
        SqlSugarClient? client = null;
        try
        {
            client = OpenClient(sourceType, connectionString);
            await Task.Run(() =>
            {
                client.Ado.Open();
                _ = client.Ado.GetDataTable("SELECT 1");
                client.Ado.Close();
            }, ct);
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw WrapOpenException(ex);
        }
        finally
        {
            client?.Dispose();
        }
    }

    private async Task<TestSourceConnectionResultDto> TestHttpAsync(string baseUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new BusinessException("请填写 HTTP 基地址");

        if (!Uri.TryCreate(baseUrl.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new BusinessException("HTTP 基地址必须是绝对 http/https URL");
        }

        using var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(5);

        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        var code = (int)response.StatusCode;
        // 2xx/3xx 以及常见鉴权响应均视为「可达」
        if (code is >= 200 and < 400 or 401 or 403)
        {
            return new TestSourceConnectionResultDto
            {
                Success = true,
                Message = $"地址可达（HTTP {code}）"
            };
        }

        return new TestSourceConnectionResultDto
        {
            Success = false,
            Message = $"地址响应异常（HTTP {code}）"
        };
    }

    /// <summary>
    /// 按类型拼装连接串；字段不完整时返回 null。
    /// </summary>
    public static string? ComposeFromBuilder(TestSourceConnectionInput input)
    {
        if (input.SourceType == DhSourceTypeEnum.Http)
        {
            return string.IsNullOrWhiteSpace(input.BaseUrl) ? null : input.BaseUrl.Trim();
        }

        var host = input.Host?.Trim();
        var database = input.Database?.Trim();
        var username = input.Username?.Trim() ?? string.Empty;
        var password = input.Password ?? string.Empty;
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(database))
            return null;

        var port = input.Port ?? DefaultPort(input.SourceType);

        return input.SourceType switch
        {
            DhSourceTypeEnum.PostgreSql =>
                $"Host={host};Port={port};Database={database};Username={username};Password={password}",
            DhSourceTypeEnum.MySql =>
                $"Server={host};Port={port};Database={database};Uid={username};Pwd={password};CharSet=utf8mb4",
            DhSourceTypeEnum.SqlServer =>
                $"Server={host},{port};Database={database};User Id={username};Password={password};TrustServerCertificate=True",
            _ => null
        };
    }

    public static int DefaultPort(DhSourceTypeEnum sourceType) => sourceType switch
    {
        DhSourceTypeEnum.PostgreSql => 5432,
        DhSourceTypeEnum.MySql => 3306,
        DhSourceTypeEnum.SqlServer => 1433,
        _ => 0
    };

    private string ResolveSecretRef(string? secretRef)
    {
        if (string.IsNullOrWhiteSpace(secretRef))
            throw new BusinessException("数据源未配置连接串或密钥引用无效");

        var raw = secretRef.Trim();
        const string configPrefix = "config:";
        const string envPrefix = "env:";

        if (raw.StartsWith(configPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var key = raw[configPrefix.Length..].Trim();
            if (string.IsNullOrWhiteSpace(key))
                throw new BusinessException("数据源未配置连接串或密钥引用无效");

            var value = configuration[key];
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessException("数据源未配置连接串或密钥引用无效");
            return value;
        }

        if (raw.StartsWith(envPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var name = raw[envPrefix.Length..].Trim();
            if (string.IsNullOrWhiteSpace(name))
                throw new BusinessException("数据源未配置连接串或密钥引用无效");

            var value = Environment.GetEnvironmentVariable(name);
            if (string.IsNullOrWhiteSpace(value))
                throw new BusinessException("数据源未配置连接串或密钥引用无效");
            return value;
        }

        throw new BusinessException("数据源未配置连接串或密钥引用无效");
    }

    private static BusinessException WrapOpenException(Exception ex)
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
            return new BusinessException("当前环境未安装对应数据库驱动，请联系管理员");
        }

        if (combined.Contains("connect", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("连接", StringComparison.Ordinal)
            || combined.Contains("timeout", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("network", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("Authentication", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("password", StringComparison.OrdinalIgnoreCase)
            || combined.Contains("login", StringComparison.OrdinalIgnoreCase))
        {
            return new BusinessException($"无法连接数据源：{TrimMessage(message)}");
        }

        return new BusinessException($"连接测试失败：{TrimMessage(message)}");
    }

    private static string FormatTestFailure(Exception ex)
    {
        if (ex is BusinessException be) return be.Message;
        return WrapOpenException(ex).Message;
    }

    private static string TrimMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return "未知错误";
        message = message.ReplaceLineEndings(" ").Trim();
        return message.Length > 200 ? message[..200] + "…" : message;
    }
}