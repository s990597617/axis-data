namespace Axis.Application.System.File.Dto;

/// <summary>
/// 文件下载载荷：流与响应头所需元数据。
/// </summary>
public sealed class FileDownloadDto
{
    /// <summary>
    /// 可读文件流；调用方负责在发送完成后释放。
    /// </summary>
    public required Stream Stream { get; init; }

    /// <summary>
    /// 下载时展示的原始文件名。
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// Content-Type。
    /// </summary>
    public required string ContentType { get; init; }
}
