using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
///     系统参数配置
/// </summary>
[SugarTable("sys_config", "系统配置")]
[SysTable]
[SugarIndex("index_{table}_code", nameof(Code), OrderByType.Asc)]
public class SysConfig : BaseEntity, ITenantOrGlobal
{
    /// <summary>
    ///     租户 Id（为空代表全局）
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }

    /// <summary>
    ///     参数分组
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "参数分组", Length = 64, IsNullable = true)]
    public string GroupCode { get; set; }

    /// <summary>
    ///     参数名称
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "参数名称", Length = 64)]
    public string Name { get; set; } = null!;

    /// <summary>
    ///     参数编码
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "参数编码", Length = 64)]
    public string Code { get; set; } = null!;

    /// <summary>
    ///     参数值
    /// </summary>
    [MaxLength(1024)]
    [SugarColumn(ColumnDescription = "参数值", Length = 1024, IsNullable = true)]
    public string Value { get; set; }

    /// <summary>
    ///     参数类型
    /// </summary>
    [SugarColumn(ColumnDescription = "参数类型")]
    public ConfigTypeEnum Type { get; set; } = ConfigTypeEnum.System;

    /// <summary>
    ///     是否内置（系统内置不允许删除）
    /// </summary>
    [SugarColumn(ColumnDescription = "是否内置")]
    public bool IsBuiltin { get; set; }

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