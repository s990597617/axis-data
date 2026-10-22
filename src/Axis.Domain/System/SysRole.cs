using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
///     系统角色
/// </summary>
[SugarTable("sys_role", "系统角色")]
[SysTable]
[SugarIndex("index_{table}_code", nameof(Code), OrderByType.Asc)]
public class SysRole : BaseEntity, ITenantId
{
    /// <summary>
    ///     租户 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }

    /// <summary>
    ///     角色名称
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "角色名称", Length = 64)]
    public string Name { get; set; } = null!;

    /// <summary>
    ///     角色编码（权限比对使用，全局唯一）
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "角色编码", Length = 64)]
    public string Code { get; set; } = null!;

    /// <summary>
    ///     数据范围
    /// </summary>
    [SugarColumn(ColumnDescription = "数据范围")]
    public DataScopeEnum DataScope { get; set; } = DataScopeEnum.Self;

    /// <summary>
    ///     排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    ///     状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    ///     备注
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "备注", Length = 256, IsNullable = true)]
    public string Remark { get; set; }
}