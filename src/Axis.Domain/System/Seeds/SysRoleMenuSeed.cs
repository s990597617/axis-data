namespace Axis.Domain.System.Seeds;

/// <summary>
///     角色-菜单 关联种子：按角色分配不同菜单权限
/// </summary>
public sealed class SysRoleMenuSeed : ISqlSugarEntitySeedData<SysRoleMenu>
{
    public IEnumerable<SysRoleMenu> HasData()
    {
        var seededMenus = new SysMenuSeed().HasData().ToArray();
        var allMenus = seededMenus.Select(menu => menu.Id).ToArray();

        // 租户系统管理员：工作台 + 个人空间 + 基础管理（不含平台系统管理）
        var sysAdminPages = new HashSet<long>
        {
            SysSeedIds.MenuHome,
            SysSeedIds.MenuPersonal,
            SysSeedIds.MenuProfile,
            SysSeedIds.MenuInbox,
            SysSeedIds.MenuBase,
            SysSeedIds.MenuUser,
            SysSeedIds.MenuRole,
            SysSeedIds.MenuOrg,
            SysSeedIds.MenuPos,
            SysSeedIds.MenuNotice
        };
        var sysAdminMenus = seededMenus
            .Where(menu => sysAdminPages.Contains(menu.Id) || sysAdminPages.Contains(menu.ParentId))
            .Select(menu => menu.Id)
            .ToArray();

        var devLeadMenus = new[]
        {
            SysSeedIds.MenuHome,
            SysSeedIds.MenuPersonal,
            SysSeedIds.MenuProfile,
            SysSeedIds.ButtonProfileQuery,
            SysSeedIds.ButtonProfileUpdate,
            SysSeedIds.ButtonProfileChangePassword,
            SysSeedIds.MenuInbox,
            SysSeedIds.ButtonInboxQuery,
            SysSeedIds.ButtonInboxRead,
            SysSeedIds.ButtonInboxReadAll,
            SysSeedIds.MenuBase,
            SysSeedIds.MenuUser,
            SysSeedIds.ButtonUserQuery,
            SysSeedIds.ButtonUserDetail
        };

        var id = SysSeedIds.RoleMenuBase;

        foreach (var menuId in allMenus)
            yield return new SysRoleMenu { Id = ++id, RoleId = SysSeedIds.RoleSuperAdmin, MenuId = menuId };

        foreach (var menuId in sysAdminMenus)
            yield return new SysRoleMenu { Id = ++id, RoleId = SysSeedIds.RoleSysAdmin, MenuId = menuId };

        foreach (var menuId in devLeadMenus)
            yield return new SysRoleMenu { Id = ++id, RoleId = SysSeedIds.RoleDevLead, MenuId = menuId };
    }
}
