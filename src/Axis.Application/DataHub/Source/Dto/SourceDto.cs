using Axis.Domain.Enums;

namespace Axis.Application.DataHub.Source.Dto;

/// <summary>
/// 数据源列表项及编辑回显数据。
/// </summary>
public sealed class SourceDto
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 编码。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 数据源类型。
    /// </summary>
    public DhSourceTypeEnum SourceType { get; set; }

    /// <summary>
    /// 密钥引用（config: / env:）。
    /// </summary>
    public string? SecretRef { get; set; }

    /// <summary>
    /// 是否已配置内联连接串（不回显明文）。
    /// </summary>
    public bool HasConnectionString { get; set; }

    /// <summary>
    /// 最近连通性测试状态。
    /// </summary>
    public DhConnectionTestStatusEnum LastTestStatus { get; set; }

    /// <summary>
    /// 最近连通性测试时间。
    /// </summary>
    public DateTime? LastTestTime { get; set; }

    /// <summary>
    /// 最近连通性测试消息。
    /// </summary>
    public string? LastTestMessage { get; set; }

    /// <summary>
    /// 是否只读。
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// 排序。
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 状态。
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 最后修改时间；从未修改时为空。
    /// </summary>
    public DateTime? ModifiedTime { get; set; }
}