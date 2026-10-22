namespace Axis.Infrastructure.Components.Caching.Services;

/// <summary>
/// Axis Framework 缓存服务接口
/// 提供统一的缓存操作抽象
/// </summary>
public interface ICacheService
{
    #region 基础操作

    /// <summary>
    /// 获取缓存值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>缓存值</returns>
    Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取缓存值，如果不存在则执行工厂方法
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="factory">工厂方法</param>
    /// <param name="expiration">过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>缓存值</returns>
    Task<T> GetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 设置缓存值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存值</param>
    /// <param name="expiration">过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 仅当键不存在时写入（原子占坑，对应 Redis <c>SET NX</c>）。
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="key">缓存键</param>
    /// <param name="value">缓存值</param>
    /// <param name="expiration">过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>写入成功（首次占坑）返回 <c>true</c>；键已存在返回 <c>false</c>。</returns>
    Task<bool> TrySetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 移除缓存
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查缓存是否存在
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否存在</returns>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取缓存剩余过期时间
    /// </summary>
    /// <param name="key">缓存键</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>剩余时间</returns>
    Task<TimeSpan> GetExpirationAsync(string key, CancellationToken cancellationToken = default);

    #endregion

    #region 批量操作

    /// <summary>
    /// 批量获取缓存值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="keys">缓存键集合</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>缓存值字典</returns>
    Task<Dictionary<string, T>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量设置缓存值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="items">缓存项字典</param>
    /// <param name="expiration">过期时间</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    Task<bool> SetManyAsync<T>(Dictionary<string, T> items, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 批量移除缓存
    /// </summary>
    /// <param name="keys">缓存键集合</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>成功移除的数量</returns>
    Task<int> RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    /// <summary>
    /// 根据模式移除缓存
    /// </summary>
    /// <param name="pattern">模式字符串</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>成功移除的数量</returns>
    Task<int> RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按前缀移除缓存。
    /// </summary>
    /// <param name="prefix">键前缀</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>命令是否执行成功（底层通常不返回精确删除数）</returns>
    Task<bool> RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>
    /// 按前缀列出缓存键。
    /// </summary>
    /// <param name="prefix">键前缀</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>匹配的键列表</returns>
    Task<IReadOnlyList<string>> GetKeysByPrefixAsync(string prefix, CancellationToken cancellationToken = default);

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
    Task<T> RefreshAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 清空所有缓存
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>是否成功</returns>
    Task<bool> FlushAllAsync(CancellationToken cancellationToken = default);

    #endregion

    #region 键管理

    /// <summary>
    /// 生成缓存键
    /// </summary>
    /// <param name="prefix">前缀</param>
    /// <param name="parts">键部分</param>
    /// <returns>缓存键</returns>
    string GenerateKey(string prefix, params object[] parts);

    /// <summary>
    /// 生成带标签的缓存键
    /// </summary>
    /// <param name="prefix">前缀</param>
    /// <param name="tags">标签</param>
    /// <param name="parts">键部分</param>
    /// <returns>缓存键</returns>
    string GenerateTaggedKey(string prefix, string[] tags, params object[] parts);

    /// <summary>
    /// 根据标签移除缓存
    /// </summary>
    /// <param name="tags">标签数组</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>成功移除的数量</returns>
    Task<int> RemoveByTagsAsync(string[] tags, CancellationToken cancellationToken = default);

    #endregion
}
