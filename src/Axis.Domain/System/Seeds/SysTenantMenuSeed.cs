namespace Axis.Domain.System.Seeds;

/// <summary>
/// 默认租户菜单启用种子：仅写入非平台菜单（工作台 / 个人空间 / 基础管理）。
/// </summary>
/// <remarks>
/// 关系 Id 与 MenuId 稳定映射，便于种子幂等更新。
/// 若库中仍有历史「系统管理」租户启用行，启动后执行一次：
/// <c>DELETE FROM sys_tenant_menu WHERE menu_id IN (SELECT id FROM sys_menu WHERE is_platform = true);</c>
/// </remarks>
public sealed class SysTenantMenuSeed : ISqlSugarEntitySeedData<SysTenantMenu>
{
    public IEnumerable<SysTenantMenu> HasData()
    {
        var menuIds = new SysMenuSeed().HasData()
            .Where(menu => !menu.IsPlatform)
            .Select(menu => menu.Id)
            .ToArray();

        foreach (var menuId in menuIds)
        {
            yield return new SysTenantMenu
            {
                // 稳定 Id：避免菜单增减导致序号错位后残留旧关系
                Id = SysSeedIds.TenantMenuBase + (menuId - 1000_0006_0000_00000L),
                TenantId = SysSeedIds.TenantDefault,
                MenuId = menuId
            };
        }
    }
}
