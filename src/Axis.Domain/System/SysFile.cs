using System.ComponentModel.DataAnnotations;
using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
/// 系统文件记录
/// </summary>
[SugarTable("sys_file", "文件记录")]
[SysTable]
[SugarIndex("index_{table}_md5", nameof(Md5), OrderByType.Asc)]
[SugarIndex("index_{table}_module", nameof(Module), OrderByType.Asc)]
public class SysFile : BaseEntity
{
    /// <summary>
    /// 文件唯一标识（雪花 ID 字符串）
    /// </summary>
    [Required]
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "文件标识", Length = 32)]
    public string FileId { get; set; } = null!;

    /// <summary>
    /// 原始文件名
    /// </summary>
    [Required]
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "原始文件名", Length = 256)]
    public string OriginalName { get; set; } = null!;

    /// <summary>
    /// 存储后的文件名
    /// </summary>
    [Required]
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "存储文件名", Length = 128)]
    public string StoredName { get; set; } = null!;

    /// <summary>
    /// 文件相对路径
    /// </summary>
    [Required]
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "相对路径", Length = 512)]
    public string RelativePath { get; set; } = null!;

    /// <summary>
    /// 可访问的 URL
    /// </summary>
    [MaxLength(1024)]
    [SugarColumn(ColumnDescription = "访问URL", Length = 1024, IsNullable = true)]
    public string Url { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    [SugarColumn(ColumnDescription = "文件大小")]
    public long Size { get; set; }

    /// <summary>
    /// 文件 ContentType
    /// </summary>
    [Required]
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "ContentType", Length = 128)]
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// 文件后缀（含点号）
    /// </summary>
    [MaxLength(16)]
    [SugarColumn(ColumnDescription = "文件后缀", Length = 16, IsNullable = true)]
    public string Extension { get; set; }

    /// <summary>
    /// 文件 MD5
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "MD5", Length = 64, IsNullable = true)]
    public string Md5 { get; set; }

    /// <summary>
    /// 业务模块标识（如 avatar、attachment 等）
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "业务模块", Length = 64, IsNullable = true)]
    public string Module { get; set; }

    /// <summary>
    /// 关联业务 ID（可选，用于绑定到具体业务记录）
    /// </summary>
    [SugarColumn(ColumnDescription = "关联业务ID", IsNullable = true)]
    public long? BizId { get; set; }

    /// <summary>
    /// 关联业务类型（配合 BizId 使用，标识是哪个业务表）
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "业务类型", Length = 64, IsNullable = true)]
    public string BizType { get; set; }

    /// <summary>
    /// 存储提供者（local / minio / oss）
    /// </summary>
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "存储提供者", Length = 32, IsNullable = true)]
    public string Provider { get; set; }
}
