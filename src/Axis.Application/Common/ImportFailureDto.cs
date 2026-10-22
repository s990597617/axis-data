namespace Axis.Application.Common;

/// <summary>
/// 导入失败的单行明细。
/// </summary>
public sealed class ImportFailureDto
{
    /// <summary>
    /// Excel 行号（含表头，首行数据一般为 2）。
    /// </summary>
    public int Row { get; set; }

    /// <summary>
    /// 失败原因。
    /// </summary>
    public string Message { get; set; } = null!;
}
