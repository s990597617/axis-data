namespace Axis.Domain.Data.System.Cache;

/// <summary>
/// 系统配置缓存门面：业务侧按配置编码读取参数值。
/// </summary>
/// <remarks>
/// 读走缓存，未命中回源数据库并回填；管理端写库成功后须调用刷新或失效。
/// 业务模块只依赖本接口，禁止在各处直接查询 <c>SysConfig</c>。
/// </remarks>
public interface ISysConfigCache
{
    /// <summary>
    /// 获取指定配置的原始字符串值。
    /// </summary>
    /// <param name="code">配置编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>配置值；配置不存在或被禁用时为 null。</returns>
    Task<string?> GetValueAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// 获取整型配置值；缺失或解析失败时返回默认值。
    /// </summary>
    /// <param name="code">配置编码。</param>
    /// <param name="defaultValue">回退默认值。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>解析后的整型值或默认值。</returns>
    Task<int> GetIntAsync(string code, int defaultValue = 0, CancellationToken ct = default);

    /// <summary>
    /// 获取布尔配置值；缺失或解析失败时返回默认值。
    /// </summary>
    /// <param name="code">配置编码。</param>
    /// <param name="defaultValue">回退默认值。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>解析后的布尔值或默认值。</returns>
    Task<bool> GetBoolAsync(string code, bool defaultValue = false, CancellationToken ct = default);

    /// <summary>
    /// 写库成功后重新加载指定配置缓存（写穿刷新）。
    /// </summary>
    /// <param name="code">配置编码。</param>
    /// <param name="ct">取消令牌。</param>
    Task RefreshAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// 失效指定配置缓存；下次读取时懒加载回填。
    /// </summary>
    /// <param name="code">配置编码。</param>
    /// <param name="ct">取消令牌。</param>
    Task InvalidateAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// 预热：将全部启用配置刷入缓存（按各自租户作用域）。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>成功写入的配置数量。</returns>
    Task<int> WarmupAsync(CancellationToken ct = default);
}
