using Axis.Domain.Enums;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.System;

namespace Axis.Infrastructure.Data.System.Repository;

/// <summary>
/// 菜单仓储实现
/// </summary>
public sealed class SysMenuRepository(ISqlSugarClient db, IServiceProvider sp)
    : BaseRepository<SysMenu>(db, sp), ISysMenuRepository, IAutoInject
{
    public async Task<IReadOnlyList<string>> GetPermissionsAsync(IReadOnlyCollection<long> menuIds, CancellationToken ct = default)
    {
        if (menuIds.Count == 0) return [];

        return await base.AsQueryable()
            .Where(m => menuIds.Contains(m.Id)
                        && m.Status == StatusEnum.Enable
                        && !SqlFunc.IsNullOrEmpty(m.Permission))
            .Select(m => m.Permission!)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SysMenu>> GetRouteMenusAsync(IReadOnlyCollection<long> menuIds, CancellationToken ct = default)
    {
        if (menuIds.Count == 0) return Array.Empty<SysMenu>();

        return await base.AsQueryable()
            .Where(m => menuIds.Contains(m.Id)
                        && m.Status == StatusEnum.Enable
                        && m.Type != MenuTypeEnum.Btn)
            .OrderBy(m => m.OrderNo)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<SysMenu>> GetAllRouteMenusAsync(CancellationToken ct = default)
        => await base.AsQueryable()
            .Where(m => m.Status == StatusEnum.Enable && m.Type != MenuTypeEnum.Btn)
            .OrderBy(m => m.OrderNo)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetButtonPermissionsAsync(long userId, bool isSuperAdmin, CancellationToken ct = default)
    {
        if (isSuperAdmin) return ["*"];

        var menuIds = await GetEffectiveUserMenuIdsAsync(userId, ct);
        if (menuIds.Count == 0) return [];

        return await base.AsQueryable()
            .Where(m => menuIds.Contains(m.Id)
                        && m.Status == StatusEnum.Enable
                        && m.Type == MenuTypeEnum.Btn
                        && !SqlFunc.IsNullOrEmpty(m.Permission))
            .Select(m => m.Permission!)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<long>> GetUserMenuIdsAsync(long userId, CancellationToken ct = default)
        => await Context.Queryable<SysUserRole>()
            .InnerJoin<SysRoleMenu>((ur, rm) => ur.RoleId == rm.RoleId)
            .Where((ur, rm) => ur.UserId == userId)
            .Select((ur, rm) => rm.MenuId)
            .Distinct()
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SysMenu>> GetUserAllMenusAsync(long userId, bool isSuperAdmin, CancellationToken ct = default)
    {
        if (isSuperAdmin)
        {
            return await base.AsQueryable()
                .Where(m => m.Status == StatusEnum.Enable)
                .OrderBy(m => m.OrderNo)
                .ToListAsync(ct);
        }

        var menuIds = await GetEffectiveUserMenuIdsAsync(userId, ct);
        if (menuIds.Count == 0) return [];

        return await base.AsQueryable()
            .Where(m => menuIds.Contains(m.Id) && m.Status == StatusEnum.Enable)
            .OrderBy(m => m.OrderNo)
            .ToListAsync(ct);
    }

    /// <summary>
    /// 角色菜单 ∩ 租户启用菜单；无租户或启用集为空则无权限。
    /// </summary>
    private async Task<IReadOnlyList<long>> GetEffectiveUserMenuIdsAsync(long userId, CancellationToken ct)
    {
        var roleMenuIds = await GetUserMenuIdsAsync(userId, ct);
        if (roleMenuIds.Count == 0) return [];

        var tenantId = await Context.Queryable<SysUser>()
            .Where(user => user.Id == userId)
            .Select(user => user.TenantId)
            .FirstAsync(ct);

        if (!tenantId.HasValue) return [];

        return await Context.Queryable<SysTenantMenu>()
            .Where(item => item.TenantId == tenantId.Value && roleMenuIds.Contains(item.MenuId))
            .Select(item => item.MenuId)
            .Distinct()
            .ToListAsync(ct);
    }
}
