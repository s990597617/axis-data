using Axis.Host.Api.Health;
using Axis.Infrastructure.Options;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace Axis.Host.Api.Health;

/// <summary>
/// Health 探针注册与映射。
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// 注册存活/就绪健康检查（数据库必选；Redis 仅在缓存类型为 redis/hybrid 时启用）。
    /// </summary>
    /// <param name="services">服务集合。</param>
    /// <param name="configuration">配置根。</param>
    public static IServiceCollection AddAxisHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var checks = services.AddHealthChecks()
            .AddCheck<SqlSugarHealthCheck>("database", tags: ["ready"]);

        var cacheType = configuration.GetSection($"{AxisCachingOptions.SectionName}:CacheType").Value
                        ?? "memory";
        if (cacheType.Equals("redis", StringComparison.OrdinalIgnoreCase)
            || cacheType.Equals("hybrid", StringComparison.OrdinalIgnoreCase))
        {
            checks.AddCheck<RedisHealthCheck>("redis", tags: ["ready"]);
        }

        return services;
    }

    /// <summary>
    /// 映射匿名可访问的健康检查端点。
    /// </summary>
    /// <param name="app">Web 应用。</param>
    /// <remarks>
    /// <c>/health/live</c>：进程存活（不探测依赖）。
    /// <c>/health/ready</c>：就绪（database，及可选 redis）。
    /// <c>/health</c>：全部检查。
    /// </remarks>
    public static WebApplication MapAxisHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false
        }).AllowAnonymous();

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready")
        }).AllowAnonymous();

        app.MapHealthChecks("/health").AllowAnonymous();
        return app;
    }
}
