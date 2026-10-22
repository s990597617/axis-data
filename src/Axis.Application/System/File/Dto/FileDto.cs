namespace Axis.Application.System.File.Dto;

/// <summary>
/// 文件管理列表出参。
/// </summary>
public sealed class FileDto
{
    /// <summary>
    /// 数据库主键。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 文件业务标识（雪花字符串）。
    /// </summary>
    public string FileId { get; set; } = null!;

    /// <summary>
    /// 原始文件名。
    /// </summary>
    public string OriginalName { get; set; } = null!;

    /// <summary>
    /// 存储后的文件名。
    /// </summary>
    public string StoredName { get; set; } = null!;

    /// <summary>
    /// 相对存储路径。
    /// </summary>
    public string RelativePath { get; set; } = null!;

    /// <summary>
    /// 可访问 URL；可为空。
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 文件大小（字节）。
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Content-Type。
    /// </summary>
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// 文件后缀（含点号）；可为空。
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// 业务模块标识；可为空。
    /// </summary>
    public string? Module { get; set; }

    /// <summary>
    /// 存储提供者（local / minio / oss）；可为空。
    /// </summary>
    public string? Provider { get; set; }

    /// <summary>
    /// 上传时间。
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 上传人账号；可为空。
    /// </summary>
    public string? Creator { get; set; }
}
