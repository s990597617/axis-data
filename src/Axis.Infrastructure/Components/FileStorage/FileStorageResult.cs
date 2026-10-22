namespace Axis.Infrastructure.Components.FileStorage;

/// <summary>
/// 文件存储结果
/// </summary>
public class FileStorageResult
{
    /// <summary>
    /// 文件唯一标识（由存储实现生成）
    /// </summary>
    public string FileId { get; set; } = null!;

    /// <summary>
    /// 原始文件名
    /// </summary>
    public string OriginalName { get; set; } = null!;

    /// <summary>
    /// 存储后的文件名（含路径前缀）
    /// </summary>
    public string StoredName { get; set; } = null!;

    /// <summary>
    /// 文件相对路径（相对于存储根目录）
    /// </summary>
    public string RelativePath { get; set; } = null!;

    /// <summary>
    /// 可访问的 URL（本地为相对路径，OSS 等为完整 URL）
    /// </summary>
    public string Url { get; set; } = null!;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 文件 ContentType
    /// </summary>
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// 文件 MD5（启用时有值）
    /// </summary>
    public string Md5 { get; set; }

    /// <summary>
    /// 文件后缀（含点号，如 .png）
    /// </summary>
    public string Extension { get; set; } = null!;
}
