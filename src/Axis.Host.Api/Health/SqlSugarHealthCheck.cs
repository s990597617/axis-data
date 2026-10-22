using Microsoft.Extensions.Diagnostics.HealthChecks;
using SqlSugar;

namespace Axis.Host.Api.Health;

/// <summary>
/// 数据库就绪探针：执行 <c>SELECT 1</c> 验证 SqlSugar 连接可用。
/// </summary>
public sealed class SqlSugarHealthCheck(ISqlSugarClient db) : IHealthCheck
{
    /// <summary>
    /// 探测数据库连通性。
    /// </summary>
    /// <param name="context">健康检查上下文。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _ = await db.Ado.GetScalarAsync("SELECT 1");
            return HealthCheckResult.Healthy("database ok");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("database unreachable", ex);
        }
    }
}
