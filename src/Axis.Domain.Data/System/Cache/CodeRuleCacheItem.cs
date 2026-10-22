using Axis.Domain.Enums;

namespace Axis.Domain.Data.System.Cache;

/// <summary>
/// 编码规则缓存快照；仅含取号 / 解析所需字段。
/// </summary>
public sealed class CodeRuleCacheItem
{
    /// <summary>
    /// 规则主键。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 租户 Id。
    /// </summary>
    public long? TenantId { get; set; }

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
    /// 启用状态。
    /// </summary>
    public StatusEnum Status { get; set; }
}
