using EasyCaching.Core;

namespace Axis.Infrastructure.Components.Caching.Services;

/// <summary>
/// Axis Framework 缓存服务实现。
/// </summary>
public class CacheService : ICacheService
{
    private readonly AxisCachingOptions _options;
    private readonly ILogger<CacheService> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IEasyCachingProvider _cachingProvider;

    /// <summary>
    /// 构造函数。
    /// 通过 <paramref name="cachingProviderFactory"/> 按 <c>CacheType</c> 挑选对应 provider
    /// （Memory 对应 <c>Memory.ProviderName</c>，Redis 对应 <c>Redis.ProviderName</c>，Hybrid 对应 <c>Hybrid.ProviderName</c>）。
    /// </summary>
    public CacheService(IEasyCachingProviderFactory cachingProviderFactory,
        ILogger<CacheService> logger,
        IHostEnvironment environment,
        IOptions<AxisCachingOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _environment = environment;
        _cachingProvider = cachingProviderFactory.GetCachingProvider(ResolveProviderName(_options));
    }

    /// <summary>
    /// 根据 <see cref="AxisCachingOptions.CacheType"/> 映射到对应 provider 名，供工厂取 provider。
    /// </summary>
    private static string ResolveProviderName(AxisCachingOptions opt)
        => (opt.CacheType ?? "memory").Trim().ToLowerInvariant() switch
        {
            "redis" => opt.Redis.ProviderName,
            "hybrid" => opt.Hybrid.ProviderName,
            _ => opt.Memory.ProviderName
        };

    #region 基础操作

    /// <summary>
    /// 获取缓存值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>缓存值</returns>
    public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _cachingProvider.GetAsync<T>(key, cancellationToken);
            return result.HasValue ? result.Value : default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取缓存失败，键: {Key}", key);
            return default;
        }
    }

    /// <summary>
    /// 获取缓存值，如果不存在则执行工厂方法
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="factory">工厂方法</param>
    /// <param name="expiration">过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>缓存值</returns>
    public async Task<T> GetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var exp = expiration ?? TimeSpan.FromMinutes(_options.Memory.DefaultExpirationMinutes);
        try
        {
            // EasyCaching 的重载带内置锁，能避免同一 key 的缓存击穿
            var result = await _cachingProvider.GetAsync(key, factory, exp, cancellationToken);
            return result.HasValue ? result.Value : default;
        }
        catch (Exception ex)
        {
            // 缓存层故障时降级直接调 factory，避免让业务链路跟缓存一起挂
            _logger.LogError(ex, "获取或创建缓存失败，降级到 factory：{Key}", key);
            return await factory();
        }
    }

    /// <summary>
    /// 设置缓存值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存值</param>
    /// <param name="expiration">过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var exp = expiration ?? TimeSpan.FromMinutes(_options.Memory.DefaultExpirationMinutes);
            await _cachingProvider.SetAsync(key, value, exp, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置缓存失败，键: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// 仅当键不存在时写入（原子占坑）。
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存值</param>
    /// <param name="expiration">过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>首次占坑成功为 <c>true</c>；键已存在为 <c>false</c>；异常时为 <c>false</c>。</returns>
    public async Task<bool> TrySetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var exp = expiration ?? TimeSpan.FromMinutes(_options.Memory.DefaultExpirationMinutes);
            // EasyCaching TrySet：Redis 走 SET NX；Memory 进程内互斥，避免 Exists+Set 竞态。
            return await _cachingProvider.TrySetAsync(key, value, exp, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "原子写入缓存失败，键: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// 移除缓存
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cachingProvider.RemoveAsync(key, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "移除缓存失败，键: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// 检查缓存是否存在
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否存在</returns>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _cachingProvider.ExistsAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "检查缓存存在性失败，键: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// 获取缓存剩余过期时间
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>剩余时间</returns>
    public async Task<TimeSpan> GetExpirationAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var seconds = await _cachingProvider.GetExpirationAsync(key, cancellationToken);
            return seconds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取缓存过期时间失败，键: {Key}", key);
            return TimeSpan.Zero;
        }
    }

    #endregion

    #region 批量操作

    /// <summary>
    /// 批量获取缓存值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="keys">缓存键集合</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>缓存值字典</returns>
    public async Task<Dictionary<string, T>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        try
        {
            var keyList = keys.ToList();
            var result = await _cachingProvider.GetAllAsync<T>(keyList, cancellationToken);

            var dictionary = new Dictionary<string, T>();
            foreach (var key in keyList)
            {
                dictionary[key] = result.TryGetValue(key, out var value) && value.HasValue ? value.Value : default;
            }

            return dictionary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量获取缓存失败");
            return new Dictionary<string, T>();
        }
    }

    /// <summary>
    /// 批量设置缓存值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="items">缓存项字典</param>
    /// <param name="expiration">过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    public async Task<bool> SetManyAsync<T>(Dictionary<string, T> items, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var exp = expiration ?? TimeSpan.FromMinutes(_options.Memory.DefaultExpirationMinutes);
            await _cachingProvider.SetAllAsync(items, exp, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量设置缓存失败");
            return false;
        }
    }

    /// <summary>
    /// 批量移除缓存
    /// </summary>
    /// <param name="keys">缓存键集合</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>成功移除的数量</returns>
    public async Task<int> RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        try
        {
            var keyList = keys.ToList();
            await _cachingProvider.RemoveAllAsync(keyList, cancellationToken);
            return keyList.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量移除缓存失败");
            return 0;
        }
    }

    /// <summary>
    /// 根据模式移除缓存。
    /// </summary>
    /// <remarks>
    /// EasyCaching 底层不返回删除数量，因此本方法只能表达"成功 / 失败"两态：
    /// <list type="bullet">
    /// <item><description>返回 1：删除命令执行成功（实际被删数量未知）</description></item>
    /// <item><description>返回 0：删除失败（见日志）</description></item>
    /// </list>
    /// </remarks>
    public async Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cachingProvider.RemoveByPatternAsync(pattern, cancellationToken);
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据模式移除缓存失败，模式: {Pattern}", pattern);
            return 0;
        }
    }

    /// <inheritdoc />
    public async Task<bool> RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cachingProvider.RemoveByPrefixAsync(prefix, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根据前缀移除缓存失败，前缀: {Prefix}", prefix);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetKeysByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        try
        {
            var keys = await _cachingProvider.GetAllKeysByPrefixAsync(prefix, cancellationToken);
            return keys?.Distinct(StringComparer.Ordinal).OrderBy(k => k, StringComparer.Ordinal).ToList()
                   ?? (IReadOnlyList<string>)Array.Empty<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "按前缀列出缓存键失败，前缀: {Prefix}", prefix);
            return Array.Empty<string>();
        }
    }

    #endregion

    #region 高级操作

    /// <summary>
    /// 刷新缓存
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="factory">工厂方法</param>
    /// <param name="expiration">过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新的缓存值</returns>
    public async Task<T> RefreshAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await factory();
            if (value != null)
            {
                await SetAsync(key, value, expiration, cancellationToken);
            }

            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新缓存失败，键: {Key}", key);
            return default;
        }
    }

    /// <summary>
    /// 清空所有缓存。
    /// 出于生产事故防护，**仅在非 Production 环境**生效；生产环境调用会记录警告并返回 <c>false</c>。
    /// 如需在生产清理数据，请用 <see cref="RemoveByPatternAsync"/> 按前缀清除。
    /// </summary>
    public async Task<bool> FlushAllAsync(CancellationToken cancellationToken = default)
    {
        if (_environment.IsProduction())
        {
            _logger.LogWarning("生产环境调用 FlushAllAsync 已被拒绝，请改用 RemoveByPatternAsync。");
            return false;
        }

        try
        {
            await _cachingProvider.FlushAsync(cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清空所有缓存失败");
            return false;
        }
    }

    #endregion

    #region 键管理

    /// <summary>
    /// 生成缓存键
    /// </summary>
    /// <param name="prefix">前缀</param>
    /// <param name="parts">键部分</param>
    /// <returns>缓存键</returns>
    public string GenerateKey(string prefix, params object[] parts)
    {
        var keyParts = new List<string> { _options.CacheKey.GlobalPrefix, prefix };
        keyParts.AddRange(parts.Select(p => p?.ToString() ?? "null"));

        var key = string.Join(_options.CacheKey.Separator, keyParts);

        // 检查键长度
        if (key.Length > _options.CacheKey.MaxKeyLength)
        {
            if (_options.CacheKey.EnableKeyHashing)
            {
                return HashKey(key);
            }
            else
            {
                _logger.LogWarning("缓存键长度超过限制: {Key}", key);
            }
        }

        return key;
    }

    /// <summary>
    /// 生成带标签的缓存键。
    /// 与旧版本差异：不再把 tag 拼进键名，而是生成普通键并由 <see cref="RemoveByTagsAsync"/> 通过 <c>tag:{name}</c> 前缀的索引集合来定位。
    /// 这里返回的是最终数据键。
    /// </summary>
    public string GenerateTaggedKey(string prefix, string[] tags, params object[] parts)
    {
        // tag 不再进键名，只作为后续 RemoveByTagsAsync 的索引依据
        _ = tags;
        return GenerateKey(prefix, parts);
    }

    /// <summary>
    /// 根据标签移除缓存。
    /// 通过模式匹配移除 <c>{GlobalPrefix}{Separator}{tag}{Separator}*</c> 的所有键，
    /// 业务侧需在 Set 时遵循 "带 tag 的键必须以 tag 为一级前缀" 的约定。
    /// </summary>
    public async Task<int> RemoveByTagsAsync(string[] tags, CancellationToken cancellationToken = default)
    {
        if (tags is null || tags.Length == 0) return 0;

        var removed = 0;
        foreach (var tag in tags.Where(t => !string.IsNullOrWhiteSpace(t)))
        {
            try
            {
                var pattern = $"{_options.CacheKey.GlobalPrefix}{_options.CacheKey.Separator}{tag}{_options.CacheKey.Separator}*";
                removed += await RemoveByPatternAsync(pattern, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据标签移除缓存失败，标签: {Tag}", tag);
            }
        }
        return removed;
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 对键进行哈希处理
    /// </summary>
    /// <param name="key">原始键</param>
    /// <returns>哈希后的键</returns>
    private static string HashKey(string key)
    {
        // 改用 SHA256 减少碰撞风险；Base64 URL-safe 截断至 43 字符（256bit/6 向上取整）
        var hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(key));
        return Convert.ToHexString(hash);
    }

    #endregion
}