using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.System.CodeRule.Dto;

/// <summary>
/// 新增或更新编码规则的输入。
/// </summary>
/// <remarks>
/// 格式校验以 Validator 为准；编码唯一性由应用服务校验。
/// </remarks>
public sealed class SaveCodeRuleInput
{
    /// <summary>
    /// 规则主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 规则编码；必填，租户内唯一。
    /// </summary>
    [Required, MaxLength(64)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 规则名称；必填。
    /// </summary>
    [Required, MaxLength(64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 前缀；对应 <c>{Prefix}</c>。
    /// </summary>
    [MaxLength(32)]
    public string? Prefix { get; set; }

    /// <summary>
    /// 日期格式；模板含 <c>{Date}</c> 时必填。
    /// </summary>
    [MaxLength(32)]
    public string? DateFormat { get; set; }

    /// <summary>
    /// 流水长度；1～16。
    /// </summary>
    public int SerialLength { get; set; } = 5;

    /// <summary>
    /// 重置周期。
    /// </summary>
    public CodeResetCycleEnum ResetCycle { get; set; } = CodeResetCycleEnum.Daily;

    /// <summary>
    /// 拼接模板。
    /// </summary>
    [Required, MaxLength(128)]
    public string Template { get; set; } = null!;

    /// <summary>
    /// 校验位算法。
    /// </summary>
    public CodeCheckAlgoEnum CheckAlgo { get; set; } = CodeCheckAlgoEnum.None;

    /// <summary>
    /// 来源类型。
    /// </summary>
    public CodeSourceTypeEnum SourceType { get; set; } = CodeSourceTypeEnum.SelfGenerated;

    /// <summary>
    /// 反向正则；可空，保存时可由模板生成。
    /// </summary>
    [MaxLength(512)]
    public string? ReverseRegex { get; set; }

    /// <summary>
    /// 匹配优先级；默认 100，数值越小越优先。
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 启用状态。
    /// </summary>
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 备注。
    /// </summary>
    [MaxLength(256)]
    public string? Remark { get; set; }
}
