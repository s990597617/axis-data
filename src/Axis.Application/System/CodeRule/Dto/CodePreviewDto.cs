using Axis.Domain.Enums;

namespace Axis.Application.System.CodeRule.Dto;

/// <summary>
/// 编码规则实时预览结果；不占用流水。
/// </summary>
public sealed class CodePreviewDto
{
    /// <summary>
    /// 预览编码。
    /// </summary>
    public string Sample { get; init; } = null!;

    /// <summary>
    /// 按模板拆出的各段。
    /// </summary>
    public IReadOnlyList<CodeSegmentDto> Segments { get; init; } = [];

    /// <summary>
    /// 预览编码总长度。
    /// </summary>
    public int Length { get; init; }

    /// <summary>
    /// 重置周期。
    /// </summary>
    public CodeResetCycleEnum ResetCycle { get; init; }

    /// <summary>
    /// 校验位算法。
    /// </summary>
    public CodeCheckAlgoEnum CheckAlgo { get; init; }

    /// <summary>
    /// 来源类型。
    /// </summary>
    public CodeSourceTypeEnum SourceType { get; init; }

    /// <summary>
    /// 按当前模板生成的反向正则。
    /// </summary>
    public string GeneratedRegex { get; init; } = null!;
}
