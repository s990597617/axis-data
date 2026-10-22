namespace Axis.Application.System.CodeRule.Dto;

/// <summary>
/// 编码模板解析出的一段。
/// </summary>
public sealed class CodeSegmentDto
{
    /// <summary>
    /// 占位符名或 <c>Literal</c>。
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// 该段渲染后的文本。
    /// </summary>
    public string Value { get; init; } = null!;
}
