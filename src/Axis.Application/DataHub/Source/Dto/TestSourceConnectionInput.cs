using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.DataHub.Source.Dto;

/// <summary>
/// 连通性测试入参（可不依赖已保存实体）。
/// </summary>
public sealed class TestSourceConnectionInput
{
    /// <summary>
    /// 数据源类型。
    /// </summary>
    public DhSourceTypeEnum SourceType { get; set; }

    /// <summary>
    /// 密钥引用（config: / env:）。
    /// </summary>
    [MaxLength(128)]
    public string? SecretRef { get; set; }

    /// <summary>
    /// 完整连接串；为空时由结构化字段拼装。
    /// </summary>
    [MaxLength(1024)]
    public string? ConnectionString { get; set; }

    /// <summary>
    /// 主机。
    /// </summary>
    [MaxLength(256)]
    public string? Host { get; set; }

    /// <summary>
    /// 端口。
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// 数据库名。
    /// </summary>
    [MaxLength(128)]
    public string? Database { get; set; }

    /// <summary>
    /// 用户名。
    /// </summary>
    [MaxLength(128)]
    public string? Username { get; set; }

    /// <summary>
    /// 密码。
    /// </summary>
    [MaxLength(256)]
    public string? Password { get; set; }

    /// <summary>
    /// HTTP / API 基地址。
    /// </summary>
    [MaxLength(1024)]
    public string? BaseUrl { get; set; }
}