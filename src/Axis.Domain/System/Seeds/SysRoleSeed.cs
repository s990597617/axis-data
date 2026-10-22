using Axis.Domain.Enums;

namespace Axis.Domain.System.Seeds;

/// <summary>
///     角色种子（模拟多级权限体系）
/// </summary>
public sealed class SysRoleSeed : ISqlSugarEntitySeedData<SysRole>
{
    public IEnumerable<SysRole> HasData()
    {
        yield return Role(SysSeedIds.RoleSuperAdmin, "超级管理员", "super-admin", DataScopeEnum.All, 1,
            "拥有系统全部权限，仅限平台运维人员使用");
        yield return Role(SysSeedIds.RoleSysAdmin, "系统管理员", "sys-admin", DataScopeEnum.All, 2,
            "负责本租户下的系统配置与用户管理");
        yield return Role(SysSeedIds.RoleDevLead, "研发主管", "dev-lead", DataScopeEnum.DeptWithChildren, 3,
            "管理研发部及下属小组的数据权限");
        yield return Role(SysSeedIds.RoleDeveloper, "开发人员", "developer", DataScopeEnum.Dept, 4,
            "普通开发岗位，仅可查看本部门数据");
        yield return Role(SysSeedIds.RoleViewer, "只读用户", "viewer", DataScopeEnum.Self, 5,
            "仅可浏览个人相关数据，无编辑权限");
    }

    private static SysRole Role(long id, string name, string code, DataScopeEnum scope, int order, string remark)
    {
        return new SysRole
        {
            Id = id,
            TenantId = SysSeedIds.TenantDefault,
            Name = name,
            Code = code,
            DataScope = scope,
            OrderNo = order,
            Status = StatusEnum.Enable,
            Remark = remark,
            CreatedBy = SysSeedIds.SeedCreatorId,
            Creator = SysSeedIds.SeedCreator,
            CreatedTime = SysSeedIds.SeedCreatedTime
        };
    }
}