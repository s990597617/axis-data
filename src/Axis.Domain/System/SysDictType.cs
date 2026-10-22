using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
///     字典类型
/// </summary>
[SugarTable("sys_dict_type", "字典类型")]
[SysTable]
[SugarIndex("index_{table}_code", nameof(Code), OrderByType.Asc)]
public class SysDictType : BaseEntity, ITenantOrGlobal
{
    /// <summary>
    ///     租户 Id（为空代表全局字典）
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }

    /// <summary>
    ///     字典名称
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "字典名称", Length = 64)]
    public string Name { get; set; } = null!;

    /// <summary>
    ///     字典编码
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "字典编码", Length = 64)]
    public string Code { get; set; } = null!;

    /// <summary>
    ///     是否内置
    /// </summary>
    [SugarColumn(ColumnDescription = "是否内置")]
    public bool IsBuiltin { get; set; }

    /// <summary>
    ///     是否租户字典；否表示全平台共用（TenantId 为空），是表示归属具体租户。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否租户字典")]
    public bool IsTenant { get; set; }

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