namespace Axis.Application.System.CodeGen.Dto;

/// <summary>
/// 代码生成能力状态。
/// </summary>
public sealed class CodeGenStatusDto
{
    public bool Enabled { get; set; }
    public bool AllowInProduction { get; set; }
    public bool IsProduction { get; set; }
    public bool CanGenerate { get; set; }
    public string RepoRoot { get; set; } = "";
    public string Message { get; set; } = "";
}
