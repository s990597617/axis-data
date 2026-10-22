using Axis.Domain.Enums;

namespace Axis.Application.DataHub.Dataset.Dto;

/// <summary>
/// 数据集列表项及编辑回显数据。
/// </summary>
public sealed class DatasetDto
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// SourceId。
    /// </summary>
    public long SourceId { get; set; }

    /// <summary>
    /// Name。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Code。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// ObjectType。
    /// </summary>
    public DhObjectTypeEnum ObjectType { get; set; }

    /// <summary>
    /// ObjectName。
    /// </summary>
    public string? ObjectName { get; set; }

    /// <summary>
    /// CustomSql。
    /// </summary>
    public string? CustomSql { get; set; }

    /// <summary>
    /// PrimaryKeyFields。
    /// </summary>
    public string? PrimaryKeyFields { get; set; }

    /// <summary>
    /// IncrementalField。
    /// </summary>
    public string? IncrementalField { get; set; }

    /// <summary>
    /// UpdateStrategy。
    /// </summary>
    public DhUpdateStrategyEnum UpdateStrategy { get; set; }

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
