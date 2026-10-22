namespace Axis.Application.System.CodeRule.Dto;

/// <summary>
/// 按反向正则解析出的规则与捕获。
/// </summary>
public sealed class CodeParseResult
{
    /// <summary>
    /// 命中的规则编码。
    /// </summary>
    public string RuleCode { get; init; } = null!;

    /// <summary>
    /// 命中的规则主键。
    /// </summary>
    public long RuleId { get; init; }

    /// <summary>
    /// 原始编码。
    /// </summary>
    public string Code { get; init; } = null!;
}
