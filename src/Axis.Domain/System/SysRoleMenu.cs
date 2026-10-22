using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
///     角色-菜单 关联
/// </summary>
[SugarTable("sys_role_menu", "角色菜单关系")]
[SysTable]
[SugarIndex("index_{table}_role", nameof(RoleId), OrderByType.Asc)]
[SugarIndex("index_{table}_menu", nameof(MenuId), OrderByType.Asc)]
public class SysRoleMenu : BaseEntityId
{
    /// <summary>
    ///     角色 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "角色Id")]
    public long RoleId { get; set; }

    /// <summary>
    ///     菜单 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单Id")]
    public long MenuId { get; set; }
}