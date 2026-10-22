using Axis.Infrastructure.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Axis.Host.Api.Health;

/// <summary>
/// Redis 就绪探针：在缓存类型为 redis / hybrid 时探测连接。
/// </summary>
public sealed class RedisHealthCheck(IOptions<AxisCachingOptions> options) : IHealthCheck
{
    /// <summary>
    /// 探测 Redis 连通性。
    /// </summary>
    /// <param name="context">健康检查上下文。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var redis = options.Value.Redis;
        if (string.IsNullOrWhiteSpace(redis.ConnectionString))
            return HealthCheckResult.Unhealthy("redis connection string missing");

        try
        {
            using var mux = await ConnectionMultiplexer.ConnectAsync(redis.ConnectionString);
            var pong = await mux.GetDatabase(redis.Database).PingAsync();
            return HealthCheckResult.Healthy($"redis ok ({pong.TotalMilliseconds:0}ms)");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("redis unreachable", ex);
        }
    }
}
