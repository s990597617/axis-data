using Axis.Domain.Enums;

namespace Axis.Application.System.CodeRule.Dto;

/// <summary>
/// 编码规则列表与编辑回显。
/// </summary>
public sealed class CodeRuleDto
{
    /// <summary>
    /// 规则主键。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 规则编码。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 规则名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 前缀。
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// 日期格式。
    /// </summary>
    public string? DateFormat { get; set; }

    /// <summary>
    /// 流水长度。
    /// </summary>
    public int SerialLength { get; set; }

    /// <summary>
    /// 重置周期。
    /// </summary>
    public CodeResetCycleEnum ResetCycle { get; set; }

    /// <summary>
    /// 拼接模板。
    /// </summary>
    public string Template { get; set; } = null!;

    /// <summary>
    /// 校验位算法。
    /// </summary>
    public CodeCheckAlgoEnum CheckAlgo { get; set; }

    /// <summary>
    /// 来源类型。
    /// </summary>
    public CodeSourceTypeEnum SourceType { get; set; }

    /// <summary>
    /// 反向正则。
    /// </summary>
    public string? ReverseRegex { get; set; }

    /// <summary>
    /// 匹配优先级。
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// 是否内置；内置规则不可删除、不可改编码。
    /// </summary>
    public bool IsBuiltin { get; set; }

    /// <summary>
    /// 启用状态。
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 最后修改时间。
    /// </summary>
    public DateTime? ModifiedTime { get; set; }
}
