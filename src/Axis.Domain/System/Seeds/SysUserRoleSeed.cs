namespace Axis.Domain.System.Seeds;

/// <summary>
///     用户-角色 关联种子
/// </summary>
public sealed class SysUserRoleSeed : ISqlSugarEntitySeedData<SysUserRole>
{
    public IEnumerable<SysUserRole> HasData()
    {
        // admin → 超级管理员
        yield return new SysUserRole { Id = SysSeedIds.UserRoleAdmin, UserId = SysSeedIds.UserAdmin, RoleId = SysSeedIds.RoleSuperAdmin };
        // 张三 → 系统管理员 + 研发主管
        yield return new SysUserRole { Id = SysSeedIds.UserRoleZhangSan, UserId = SysSeedIds.UserZhangSan, RoleId = SysSeedIds.RoleSysAdmin };
        // 李四 → 开发人员
        yield return new SysUserRole { Id = SysSeedIds.UserRoleLiSi, UserId = SysSeedIds.UserLiSi, RoleId = SysSeedIds.RoleDeveloper };
        // 王五 → 开发人员
        yield return new SysUserRole { Id = SysSeedIds.UserRoleWangWu, UserId = SysSeedIds.UserWangWu, RoleId = SysSeedIds.RoleDeveloper };
        // 赵六 → 只读用户
        yield return new SysUserRole { Id = SysSeedIds.UserRoleZhaoLiu, UserId = SysSeedIds.UserZhaoLiu, RoleId = SysSeedIds.RoleViewer };
        // 孙七 → 只读用户
        yield return new SysUserRole { Id = SysSeedIds.UserRoleSunQi, UserId = SysSeedIds.UserSunQi, RoleId = SysSeedIds.RoleViewer };
    }
}