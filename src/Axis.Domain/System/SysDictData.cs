using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
///     字典数据项
/// </summary>
[SugarTable("sys_dict_data", "字典数据")]
[SysTable]
[SugarIndex("index_{table}_type", nameof(DictTypeId), OrderByType.Asc)]
[SugarIndex("index_{table}_value", nameof(Value), OrderByType.Asc)]
public class SysDictData : BaseEntity
{
    /// <summary>
    ///     所属字典类型 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "字典类型Id")]
    public long DictTypeId { get; set; }

    /// <summary>
    ///     字典项标签
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "字典项标签", Length = 64)]
    public string Label { get; set; } = null!;

    /// <summary>
    ///     字典项值
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "字典项值", Length = 64)]
    public string Value { get; set; } = null!;

    /// <summary>
    ///     标签样式（前端回显 tag 颜色等，如 success / warning）
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "标签样式", Length = 32, IsNullable = true)]
    public string TagStyle { get; set; }

    /// <summary>
    ///     回显样式（class / 样式名）
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "回显样式", Length = 32, IsNullable = true)]
    public string ListClass { get; set; }

    /// <summary>
    ///     是否默认
    /// </summary>
    [SugarColumn(ColumnDescription = "是否默认")]
    public bool IsDefault { get; set; }

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