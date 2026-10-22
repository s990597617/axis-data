using Axis.Infrastructure.Components.Caching.Services;
using EasyCaching.Serialization.SystemTextJson.Configurations;

namespace Axis.Infrastructure.Components.Caching;

/// <summary>
/// EasyCaching 服务集合扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Axis Framework EasyCaching 支持。
    /// 配置注册 Memory / Redis / Hybrid 提供者，
    /// </summary>
    /// <param name="services">服务集合</param>
    public static IServiceCollection AddAxisEasyCaching(this IServiceCollection services)
    {
        var options = GlobalContext.BootstrapCaching;

        // 绑定 IOptions<AxisCachingOptions>，CacheService 通过 IOptions 拿到与 GlobalContext 一致的配置
        var config =GlobalContext.Configuration;
        services.Configure<AxisCachingOptions>(config.GetSection(AxisCachingOptions.SectionName));

        // 大小写无关的 CacheType 解析，避免 "Redis" / "REDIS" 静默失效
        var cacheType = NormalizeCacheType(options.CacheType);

        services.AddEasyCaching(easyCachingOptions =>
        {
            switch (cacheType)
            {
                case CacheProviderType.Memory:
                    easyCachingOptions.UseInMemory(options.Memory.ProviderName);
                    break;

                case CacheProviderType.Redis:
                    easyCachingOptions.UseRedis(c =>
                    {
                        c.DBConfig.Configuration = options.Redis.ConnectionString;
                        c.DBConfig.Database = options.Redis.Database;
                        c.DBConfig.KeyPrefix = options.Redis.KeyPrefix;
                    }, options.Redis.ProviderName)
                    .WithSystemTextJson(options.Redis.ProviderName);
                    break;

                case CacheProviderType.Hybrid:
                    // Hybrid 需要本地 + 分布式两个 provider 都被注册
                    easyCachingOptions.UseInMemory(options.Hybrid.LocalCacheProviderName);
                    easyCachingOptions.UseRedis(c =>
                    {
                        c.DBConfig.Configuration = options.Redis.ConnectionString;
                        c.DBConfig.Database = options.Redis.Database;
                        c.DBConfig.KeyPrefix = options.Redis.KeyPrefix;
                    }, options.Hybrid.DistributedCacheProviderName)
                    .WithSystemTextJson(options.Hybrid.DistributedCacheProviderName);
                    easyCachingOptions.UseHybrid(c =>
                    {
                        c.TopicName = options.Hybrid.TopicName;
                        c.LocalCacheProviderName = options.Hybrid.LocalCacheProviderName;
                        c.DistributedCacheProviderName = options.Hybrid.DistributedCacheProviderName;
                    }, options.Hybrid.ProviderName);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Redis / Hybrid 场景启用 PubSub 以便跨节点失效
            if (cacheType is CacheProviderType.Redis or CacheProviderType.Hybrid)
            {
                easyCachingOptions.WithRedisBus(bus =>
                {
                    bus.Database = options.Redis.Database;
                    bus.Configuration = options.Redis.ConnectionString;
                });
            }
        });

        // 显式注册 ICacheService，保证单一注册源（CacheService 不再标 IAutoInject，避免被扫描器重复注册）
        services.AddScoped<ICacheService, CacheService>();

        return services;
    }

    /// <summary>
    /// 大小写无关地解析缓存类型字符串。
    /// </summary>
    private static CacheProviderType NormalizeCacheType(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return CacheProviderType.Memory;
        return raw.Trim().ToLowerInvariant() switch
        {
            "memory" => CacheProviderType.Memory,
            "redis" => CacheProviderType.Redis,
            "hybrid" => CacheProviderType.Hybrid,
            _ => CacheProviderType.Memory
        };
    }

    /// <summary>
    /// 缓存类型枚举
    /// </summary>
    private enum CacheProviderType
    {
        Memory,
        Redis,
        Hybrid
    }
}