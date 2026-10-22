namespace Axis.Application.System.File.Dto;

/// <summary>
/// 文件上传结果。
/// </summary>
public sealed class UploadFileResultDto
{
    /// <summary>
    /// 文件唯一标识。
    /// </summary>
    public string FileId { get; set; } = null!;

    /// <summary>
    /// 原始文件名。
    /// </summary>
    public string OriginalName { get; set; } = null!;

    /// <summary>
    /// 可访问 URL。
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// 相对存储路径。
    /// </summary>
    public string RelativePath { get; set; } = null!;

    /// <summary>
    /// 文件大小（字节）。
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Content-Type。
    /// </summary>
    public string ContentType { get; set; } = null!;
}
