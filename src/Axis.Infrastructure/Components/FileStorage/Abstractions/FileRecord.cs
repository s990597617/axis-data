namespace Axis.Infrastructure.Components.FileStorage.Abstractions;

/// <summary>
/// 文件记录数据模型（Infrastructure 层抽象，与领域实体解耦）。
/// <para>由仓储实现负责将此模型映射到具体的数据库实体。</para>
/// </summary>
public class FileRecord
{
    /// <summary>
    /// 文件唯一标识
    /// </summary>
    public string FileId { get; set; } = null!;

    /// <summary>
    /// 原始文件名
    /// </summary>
    public string OriginalName { get; set; } = null!;

    /// <summary>
    /// 存储后的文件名
    /// </summary>
    public string StoredName { get; set; } = null!;

    /// <summary>
    /// 文件相对路径
    /// </summary>
    public string RelativePath { get; set; } = null!;

    /// <summary>
    /// 可访问 URL
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// ContentType
    /// </summary>
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// 文件后缀
    /// </summary>
    public string Extension { get; set; }

    /// <summary>
    /// MD5
    /// </summary>
    public string Md5 { get; set; }

    /// <summary>
    /// 业务模块
    /// </summary>
    public string Module { get; set; }

    /// <summary>
    /// 关联业务 ID
    /// </summary>
    public long? BizId { get; set; }

    /// <summary>
    /// 关联业务类型
    /// </summary>
    public string BizType { get; set; }

    /// <summary>
    /// 存储提供者
    /// </summary>
    public string Provider { get; set; }
}
