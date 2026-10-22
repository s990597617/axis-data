namespace Axis.Domain.Data.System.Cache;

/// <summary>
/// 编码规则定义缓存。只缓存规则，不缓存流水。
/// </summary>
public interface ISysCodeRuleCache
{
    /// <summary>
    /// 按规则编码读取启用中的规则快照。
    /// </summary>
    /// <param name="code">规则编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>启用规则；不存在或已禁用时为 null。</returns>
    Task<CodeRuleCacheItem?> GetEnabledAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// 当前租户下全部启用规则，按优先级升序。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>启用规则列表。</returns>
    Task<IReadOnlyList<CodeRuleCacheItem>> ListEnabledAsync(CancellationToken ct = default);

    /// <summary>
    /// 写库成功后刷新指定编码的缓存。
    /// </summary>
    /// <param name="code">规则编码。</param>
    /// <param name="ct">取消令牌。</param>
    Task RefreshAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// 失效指定编码及启用列表缓存。
    /// </summary>
    /// <param name="code">规则编码。</param>
    /// <param name="ct">取消令牌。</param>
    Task InvalidateAsync(string code, CancellationToken ct = default);
}
