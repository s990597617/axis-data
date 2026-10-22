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
    /// Name。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Code。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// SourceType。
    /// </summary>
    public DhSourceTypeEnum SourceType { get; set; }

    /// <summary>
    /// SecretRef。
    /// </summary>
    public string? SecretRef { get; set; }

    /// <summary>
    /// IsReadOnly。
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// OrderNo。
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// Status。
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// Remark。
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
