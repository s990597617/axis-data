namespace Axis.Application.System.CodeGen.Dto;

/// <summary>
/// 预览用业务字段摘要。
/// </summary>
public sealed class CodeGenFieldDto
{
    public string Name { get; set; } = "";
    public string CsType { get; set; } = "";
    public string TsType { get; set; } = "";
    public string Label { get; set; } = "";
}
