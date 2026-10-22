namespace Axis.Infrastructure.Components.FastEndpoints.Permission;

/// <summary>
/// Axis 权限校验契约。
/// <para>
/// 框架内核不内置具体权限模型，由业务侧 / 身份模块负责实现并注册为 Scoped 服务。
/// 典型实现会基于当前登录用户（<see cref="Author.ICurrentAccount"/>）
/// 从数据库或缓存拉取其所拥有的权限码集合进行判定。
/// </para>
/// <para>
/// 实现类应保证：
/// </para>
/// <list type="bullet">
///   <item><description>对匿名 / 未登录用户一律返回 <c>false</c>。</description></item>
///   <item><description>对超级管理员可在实现内部短路直接放行，避免重复查询。</description></item>
///   <item><description>方法是请求幂等的，多次调用结果一致。</description></item>
/// </list>
/// </summary>
public interface IPermissionService
{
    /// <summary>
    /// 判断当前用户是否同时具备所有给定权限码（AND 语义）。
    /// </summary>
    /// <param name="codes">待校验的权限码集合，空集合返回 <c>true</c>（不限制）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>全部命中返回 <c>true</c>；任一缺失返回 <c>false</c>。</returns>
    Task<bool> HasAllAsync(IReadOnlyList<string> codes, CancellationToken ct = default);

    /// <summary>
    /// 判断当前用户是否至少具备其中一个权限码（OR 语义）。
    /// </summary>
    /// <param name="codes">待校验的权限码集合，空集合返回 <c>true</c>（不限制）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>任一命中返回 <c>true</c>；全部未命中返回 <c>false</c>。</returns>
    Task<bool> HasAnyAsync(IReadOnlyList<string> codes, CancellationToken ct = default);
}
