namespace Axis.Application.System.CodeRule.Dto;

/// <summary>
/// 取号成功后的完整编码与拆段。
/// </summary>
public sealed class CodeGenerateResult
{
    /// <summary>
    /// 完整编码。
    /// </summary>
    public string Value { get; init; } = null!;

    /// <summary>
    /// 所用规则主键。
    /// </summary>
    public long RuleId { get; init; }

    /// <summary>
    /// 所用规则编码。
    /// </summary>
    public string RuleCode { get; init; } = null!;

    /// <summary>
    /// 模板各段。
    /// </summary>
    public IReadOnlyList<CodeSegmentDto> Segments { get; init; } = [];
}
