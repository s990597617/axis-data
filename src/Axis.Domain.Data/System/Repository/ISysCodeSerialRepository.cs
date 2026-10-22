namespace Axis.Domain.Data.System.Repository;

/// <summary>
/// 编码规则流水桶仓储。
/// </summary>
/// <remarks>
/// 仅负责原子递增与窥视下一号；业务渲染在 <c>ICodeGenerator</c>。
/// </remarks>
public interface ISysCodeSerialRepository
{
    /// <summary>
    /// 在指定流水桶上原子加一并返回新值；桶不存在时插入 <c>1</c>。
    /// </summary>
    /// <param name="tenantId">取号租户；须与规则一致。</param>
    /// <param name="ruleId">规则主键。</param>
    /// <param name="periodKey">周期键。</param>
    /// <param name="scopeKey">范围键；第一期为下划线。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>占用后的流水值。</returns>
    Task<long> IncrementAsync(
        long? tenantId,
        long ruleId,
        string periodKey,
        string scopeKey,
        CancellationToken ct = default);

    /// <summary>
    /// 窥视下一号，不写库。无桶时返回 1。
    /// </summary>
    /// <param name="tenantId">取号租户。</param>
    /// <param name="ruleId">规则主键。</param>
    /// <param name="periodKey">周期键。</param>
    /// <param name="scopeKey">范围键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>若现在取号将得到的流水值。</returns>
    Task<long> PeekNextAsync(
        long? tenantId,
        long ruleId,
        string periodKey,
        string scopeKey,
        CancellationToken ct = default);
}
