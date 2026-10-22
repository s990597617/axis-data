using Axis.Domain.System;

namespace Axis.Domain.Data.System.Repository;

/// <summary>
/// 菜单仓储
/// </summary>
public interface ISysMenuRepository : IRepository<SysMenu>
{
    /// <summary>
    /// 获取指定菜单集合中已启用且具备权限标识的权限码
    /// </summary>
    Task<IReadOnlyList<string>> GetPermissionsAsync(IReadOnlyCollection<long> menuIds, CancellationToken ct = default);

    /// <summary>
    /// 获取指定菜单集合（用于组装前端路由；目录 + 菜单，不含按钮）
    /// </summary>
    Task<IReadOnlyList<SysMenu>> GetRouteMenusAsync(IReadOnlyCollection<long> menuIds, CancellationToken ct = default);

    /// <summary>
    /// 获取全部已启用的路由菜单（超级管理员使用）
    /// </summary>
    Task<IReadOnlyList<SysMenu>> GetAllRouteMenusAsync(CancellationToken ct = default);

    /// <summary>
    /// 获取用户拥有的按钮级权限集合（去重）。
    /// <para>超级管理员直接返回通配 <c>*</c>。</para>
    /// </summary>
    Task<IReadOnlyList<string>> GetButtonPermissionsAsync(long userId, bool isSuperAdmin, CancellationToken ct = default);

    /// <summary>
    /// 获取用户可见菜单 Id 列表（包含按钮，方便用于权限判定；不含按钮的树请使用 <see cref="GetRouteMenusAsync"/>）。
    /// </summary>
    Task<IReadOnlyList<long>> GetUserMenuIdsAsync(long userId, CancellationToken ct = default);

    /// <summary>
    /// 获取用户可见的全部启用菜单（目录 / 菜单 / 按钮），由上层构建菜单树。
    /// <para>超级管理员返回全部启用菜单。</para>
    /// </summary>
    Task<IReadOnlyList<SysMenu>> GetUserAllMenusAsync(long userId, bool isSuperAdmin, CancellationToken ct = default);
}
