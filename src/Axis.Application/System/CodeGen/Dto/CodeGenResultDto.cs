namespace Axis.Application.System.CodeGen.Dto;

/// <summary>
/// 预览 / 生成结果。
/// </summary>
public sealed class CodeGenResultDto
{
    public string Area { get; set; } = "";
    public string Module { get; set; } = "";
    public string CnName { get; set; } = "";
    public string RoutePrefix { get; set; } = "";
    public string FieldSource { get; set; } = "";
    public bool DryRun { get; set; }
    public List<CodeGenFieldDto> Fields { get; set; } = [];
    public List<CodeGenFileActionDto> Actions { get; set; } = [];
    public List<string> Notes { get; set; } = [];
}
