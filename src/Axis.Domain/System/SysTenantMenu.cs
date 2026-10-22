using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
/// 租户菜单关系
/// </summary>
[SugarTable("sys_tenant_menu", "租户菜单关系")]
[SysTable]
[SugarIndex("uk_{table}_tenant_menu", nameof(TenantId), OrderByType.Asc, nameof(MenuId), OrderByType.Asc, true)]
[SugarIndex("index_{table}_tenant", nameof(TenantId), OrderByType.Asc)]
[SugarIndex("index_{table}_menu", nameof(MenuId), OrderByType.Asc)]
public class SysTenantMenu : BaseEntityId
{
    /// <summary>
    /// 租户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long TenantId { get; set; }

    /// <summary>
    /// 菜单Id
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单Id")]
    public long MenuId { get; set; }
}
