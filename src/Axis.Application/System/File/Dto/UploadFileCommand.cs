namespace Axis.Application.System.File.Dto;

/// <summary>
/// 文件上传命令（与 HTTP 解耦，由 Endpoint 从 IFormFile 适配）。
/// </summary>
public sealed class UploadFileCommand
{
    /// <summary>
    /// 文件流。
    /// </summary>
    public required Stream Stream { get; init; }

    /// <summary>
    /// 原始文件名。
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Content-Type。
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// 文件大小（字节）。
    /// </summary>
    public long Size { get; init; }

    /// <summary>
    /// 业务模块归档目录（如 avatar）；可为空。
    /// </summary>
    public string? Module { get; init; }
}
