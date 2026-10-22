namespace Axis.Application.System.CodeGen.Dto;

/// <summary>
/// 单文件生成动作（预览时 Content 为将写入/合并后的完整文本）。
/// </summary>
public sealed class CodeGenFileActionDto
{
    public string Path { get; set; } = "";
    public string Action { get; set; } = "";
    public string? Detail { get; set; }

    /// <summary>
    /// 将生成的完整文件内容；SKIP 时为空。
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 文件语言提示（vue / ts / csharp / scss…）。
    /// </summary>
    public string Language { get; set; } = "text";
}
