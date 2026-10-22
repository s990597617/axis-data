namespace Axis.Application.Common;

/// <summary>
/// Excel 文件下载载荷。
/// </summary>
public sealed class ExcelFileResult
{
    /// <summary>
    /// 文件字节内容。
    /// </summary>
    public required byte[] Content { get; init; }

    /// <summary>
    /// 下载文件名（含 .xlsx 后缀）。
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// MIME 类型；固定为 OpenXML xlsx。
    /// </summary>
    public string ContentType { get; init; } =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
}
