using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
/// 统一编码规则。
/// </summary>
/// <remarks>
/// 业务模块通过规则编码调用 <c>ICodeGenerator</c> 取号；禁止各模块自行拼日期流水。
/// </remarks>
[SugarTable("sys_code_rule", "编码规则")]
[SysTable]
[SugarIndex("index_{table}_code", nameof(Code), OrderByType.Asc)]
public class SysCodeRule : BaseEntity, ITenantId
{
    /// <summary>
    /// 租户 Id；第一期按租户隔离规则。
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }

    /// <summary>
    /// 规则编码；租户内唯一，业务取号键。
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "规则编码", Length = 64)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 规则名称。
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "规则名称", Length = 64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 条码前缀；对应模板 <c>{Prefix}</c>。
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "前缀", Length = 32, IsNullable = true)]
    public string? Prefix { get; set; }

    /// <summary>
    /// 日期格式；对应模板 <c>{Date}</c>，如 <c>yyyyMMdd</c>。
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "日期格式", Length = 32, IsNullable = true)]
    public string? DateFormat { get; set; }

    /// <summary>
    /// 流水号左侧补零长度；取值 1～16。
    /// </summary>
    [SugarColumn(ColumnDescription = "流水长度")]
    public int SerialLength { get; set; } = 5;

    /// <summary>
    /// 流水重置周期。
    /// </summary>
    [SugarColumn(ColumnDescription = "重置周期")]
    public CodeResetCycleEnum ResetCycle { get; set; } = CodeResetCycleEnum.Daily;

    /// <summary>
    /// 拼接模板，如 <c>{Prefix}-{Date}-{Serial}</c>。
    /// </summary>
    [Required]
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "拼接模板", Length = 128)]
    public string Template { get; set; } = null!;

    /// <summary>
    /// 校验位算法；无算法时模板勿含 <c>{Check}</c>。
    /// </summary>
    [SugarColumn(ColumnDescription = "校验位算法")]
    public CodeCheckAlgoEnum CheckAlgo { get; set; } = CodeCheckAlgoEnum.None;

    /// <summary>
    /// 来源类型：本厂生成或外部传入。
    /// </summary>
    [SugarColumn(ColumnDescription = "来源类型")]
    public CodeSourceTypeEnum SourceType { get; set; } = CodeSourceTypeEnum.SelfGenerated;

    /// <summary>
    /// 反向正则；用于扫码解析与外来码校验。
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "反向正则", Length = 512, IsNullable = true)]
    public string? ReverseRegex { get; set; }

    /// <summary>
    /// 匹配优先级；数值越小，扫码反解时越优先。
    /// </summary>
    [SugarColumn(ColumnDescription = "匹配优先级")]
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 是否内置种子规则；内置规则不允许删除，编码不允许修改。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否内置")]
    public bool IsBuiltin { get; set; }

    /// <summary>
    /// 启用状态；禁用后禁止取号。
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 备注。
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "备注", Length = 256, IsNullable = true)]
    public string? Remark { get; set; }
}
