namespace Axis.Application.System.CodeRule.Dto;

/// <summary>
/// 按模板生成的反向正则。
/// </summary>
public sealed class CodeRegexDto
{
    /// <summary>
    /// 正则文本。
    /// </summary>
    public string Pattern { get; init; } = null!;
}
