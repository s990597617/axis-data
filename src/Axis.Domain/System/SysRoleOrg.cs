using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
///     角色-机构 关联（自定义数据范围时使用）
/// </summary>
[SugarTable("sys_role_org", "角色机构关系")]
[SysTable]
[SugarIndex("index_{table}_role", nameof(RoleId), OrderByType.Asc)]
[SugarIndex("index_{table}_org", nameof(OrgId), OrderByType.Asc)]
public class SysRoleOrg : BaseEntityId
{
    /// <summary>
    ///     角色 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "角色Id")]
    public long RoleId { get; set; }

    /// <summary>
    ///     机构 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "机构Id")]
    public long OrgId { get; set; }
}