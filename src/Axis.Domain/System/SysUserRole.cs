using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
///     用户-角色 关联
/// </summary>
[SugarTable("sys_user_role", "用户角色关系")]
[SysTable]
[SugarIndex("index_{table}_user", nameof(UserId), OrderByType.Asc)]
[SugarIndex("index_{table}_role", nameof(RoleId), OrderByType.Asc)]
public class SysUserRole : BaseEntityId
{
    /// <summary>
    ///     用户 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id")]
    public long UserId { get; set; }

    /// <summary>
    ///     角色 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "角色Id")]
    public long RoleId { get; set; }
}