namespace Axis.Infrastructure.Options;

/// <summary>
/// Axis Framework 缓存配置选项
/// </summary>
public class AxisCachingOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "AxisCaching";

    /// <summary>
    /// 缓存类型，memory：内存缓存，redis：redis缓存，hybrid：同时兼容
    /// </summary>
    public string CacheType { get; set; } = "memory";

    /// <summary>
    /// 内存缓存配置
    /// </summary>
    public MemoryCacheOptions Memory { get; set; } = new();

    /// <summary>
    /// Redis 缓存配置
    /// </summary>
    public RedisCacheOptions Redis { get; set; } = new();

    /// <summary>
    /// 混合缓存配置
    /// </summary>
    public HybridCacheOptions Hybrid { get; set; } = new();

    /// <summary>
    /// 缓存键配置
    /// </summary>
    public CacheKeyOptions CacheKey { get; set; } = new();
}

/// <summary>
/// 内存缓存配置选项
/// </summary>
public class MemoryCacheOptions
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    public string ProviderName { get; set; } = "memory";

    /// <summary>
    /// 默认过期时间（分钟）
    /// </summary>
    public int DefaultExpirationMinutes { get; set; } = 30;
}

/// <summary>
/// Redis 缓存配置选项
/// </summary>
public class RedisCacheOptions
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    public string ProviderName { get; set; } = "redis";

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// 数据库编号
    /// </summary>
    public int Database { get; set; } = 0;

    /// <summary>
    /// 键前缀
    /// </summary>
    public string KeyPrefix { get; set; } = "Axis:";
}

/// <summary>
/// 混合缓存配置选项
/// </summary>
public class HybridCacheOptions
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    public string ProviderName { get; set; } = "hybrid";

    /// <summary>
    /// 本地缓存提供者名称
    /// </summary>
    public string LocalCacheProviderName { get; set; } = "memory";

    /// <summary>
    /// 分布式缓存提供者名称
    /// </summary>
    public string DistributedCacheProviderName { get; set; } = "redis";

    /// <summary>
    /// 主题名称
    /// </summary>
    public string TopicName { get; set; } = "Axis-cache-topic";
}

/// <summary>
/// 缓存键配置选项
/// </summary>
public class CacheKeyOptions
{
    /// <summary>
    /// 全局键前缀
    /// </summary>
    public string GlobalPrefix { get; set; } = "Axis";

    /// <summary>
    /// 键分隔符
    /// </summary>
    public string Separator { get; set; } = ":";

    /// <summary>
    /// 最大键长度
    /// </summary>
    public int MaxKeyLength { get; set; } = 250;

    /// <summary>
    /// 是否启用键哈希
    /// </summary>
    public bool EnableKeyHashing { get; set; } = false;
}