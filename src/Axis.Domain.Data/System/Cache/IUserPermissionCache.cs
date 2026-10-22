namespace Axis.Domain.Data.System.Cache;

/// <summary>
/// 用户按钮权限码缓存门面：登录写入、鉴权读取、授权变更失效。
/// </summary>
/// <remarks>
/// 权限码与登录资料 <c>Buttons</c>、端点 <c>RequirePermission</c> 同源。
/// 业务侧只依赖本接口；禁止在鉴权路径直接查菜单表。
/// </remarks>
public interface IUserPermissionCache
{
    /// <summary>
    /// 获取用户按钮权限码；未命中时回源数据库并回填缓存。
    /// </summary>
    /// <param name="userId">用户主键。</param>
    /// <param name="isSuperAdmin">是否超级管理员；为真时结果为通配 <c>*</c>。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>去重后的权限码列表；无权限时为空列表。</returns>
    Task<IReadOnlyList<string>> GetAsync(long userId, bool isSuperAdmin, CancellationToken ct = default);

    /// <summary>
    /// 将已知权限码写入缓存（登录 / 资料装配后调用，避免鉴权再查库）。
    /// </summary>
    /// <param name="userId">用户主键。</param>
    /// <param name="codes">权限码集合。</param>
    /// <param name="ct">取消令牌。</param>
    Task SetAsync(long userId, IReadOnlyList<string> codes, CancellationToken ct = default);

    /// <summary>
    /// 从数据库重新加载并写穿刷新指定用户权限缓存。
    /// </summary>
    /// <param name="userId">用户主键。</param>
    /// <param name="isSuperAdmin">是否超级管理员。</param>
    /// <param name="ct">取消令牌。</param>
    Task RefreshAsync(long userId, bool isSuperAdmin, CancellationToken ct = default);

    /// <summary>
    /// 失效指定用户权限缓存；下次读取时懒加载回填。
    /// </summary>
    /// <param name="userId">用户主键。</param>
    /// <param name="ct">取消令牌。</param>
    Task InvalidateAsync(long userId, CancellationToken ct = default);

    /// <summary>
    /// 角色菜单授权变更后，失效该角色下全部用户的权限缓存。
    /// </summary>
    /// <param name="roleId">角色主键。</param>
    /// <param name="ct">取消令牌。</param>
    Task InvalidateByRoleAsync(long roleId, CancellationToken ct = default);

    /// <summary>
    /// 租户菜单启用集变更后，失效该租户下全部用户的权限缓存。
    /// </summary>
    /// <param name="tenantId">租户主键。</param>
    /// <param name="ct">取消令牌。</param>
    Task InvalidateByTenantAsync(long tenantId, CancellationToken ct = default);
}
