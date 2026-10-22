namespace Axis.Domain.Data.System.Cache;

/// <summary>
/// 字典缓存门面：业务侧按字典编码读取启用数据项与翻译标签。
/// </summary>
/// <remarks>
/// 读走缓存，未命中回源数据库并回填；管理端写库成功后须调用刷新或失效。
/// 业务模块只依赖本接口，禁止直接查询 <c>SysDictData</c> 做选项翻译。
/// <para>
/// 缓存键按归属拆分：平台字典 → <c>g</c>；租户字典 → 租户 Id。
/// 读取时先租户键，未命中再回退全局 <c>g</c>。
/// </para>
/// </remarks>
public interface ISysDictCache
{
    /// <summary>
    /// 获取指定字典类型下的启用数据项（按排序号升序）。
    /// </summary>
    /// <param name="code">字典类型编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>启用数据项只读列表；类型不存在或无启用项时为空列表。</returns>
    Task<IReadOnlyList<DictItemDto>> GetItemsAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// 将字典值翻译为展示标签。
    /// </summary>
    /// <param name="code">字典类型编码。</param>
    /// <param name="value">字典存储值。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>匹配的标签；未匹配时为 null。</returns>
    Task<string?> GetLabelAsync(string code, string value, CancellationToken ct = default);

    /// <summary>
    /// 写库成功后重新加载指定字典类型缓存（写穿刷新）。
    /// </summary>
    /// <param name="code">字典类型编码。</param>
    /// <param name="tenantId">类型归属租户；null 表示平台全局（scope = g）。</param>
    /// <param name="ct">取消令牌。</param>
    Task RefreshAsync(string code, long? tenantId, CancellationToken ct = default);

    /// <summary>
    /// 失效指定字典类型缓存；下次读取时懒加载回填。
    /// </summary>
    /// <param name="code">字典类型编码。</param>
    /// <param name="tenantId">类型归属租户；null 表示平台全局（scope = g）。</param>
    /// <param name="ct">取消令牌。</param>
    Task InvalidateAsync(string code, long? tenantId, CancellationToken ct = default);

    /// <summary>
    /// 预热：将全部启用字典类型刷入缓存（按各自归属 scope）。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>成功写入的类型数量。</returns>
    Task<int> WarmupAsync(CancellationToken ct = default);
}
