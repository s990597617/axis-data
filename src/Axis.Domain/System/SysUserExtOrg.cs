using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
///     用户-扩展机构（用户可兼任多个部门）
/// </summary>
[SugarTable("sys_user_ext_org", "用户扩展机构")]
[SysTable]
[SugarIndex("index_{table}_user", nameof(UserId), OrderByType.Asc)]
[SugarIndex("index_{table}_org", nameof(OrgId), OrderByType.Asc)]
public class SysUserExtOrg : BaseEntityId
{
    /// <summary>
    ///     用户 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id")]
    public long UserId { get; set; }

    /// <summary>
    ///     机构 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "机构Id")]
    public long OrgId { get; set; }

    /// <summary>
    ///     岗位 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "岗位Id", IsNullable = true)]
    public long? PosId { get; set; }
}