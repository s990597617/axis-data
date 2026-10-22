using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.DataHub.Dataset.Dto;

/// <summary>
/// 新增或更新数据集的输入数据。
/// </summary>
public sealed class SaveDatasetInput
{
    /// <summary>
    /// 主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// SourceId。
    /// </summary>
    public long SourceId { get; set; }

    /// <summary>
    /// Name。
    /// </summary>
    [Required, MaxLength(64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Code。
    /// </summary>
    [Required, MaxLength(64)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// ObjectType。
    /// </summary>
    public DhObjectTypeEnum ObjectType { get; set; }

    /// <summary>
    /// ObjectName。
    /// </summary>
    [MaxLength(256)]
    public string? ObjectName { get; set; }

    /// <summary>
    /// CustomSql。
    /// </summary>
    public string? CustomSql { get; set; }

    /// <summary>
    /// PrimaryKeyFields。
    /// </summary>
    [MaxLength(256)]
    public string? PrimaryKeyFields { get; set; }

    /// <summary>
    /// IncrementalField。
    /// </summary>
    [MaxLength(128)]
    public string? IncrementalField { get; set; }

    /// <summary>
    /// UpdateStrategy。
    /// </summary>
    public DhUpdateStrategyEnum UpdateStrategy { get; set; }

    /// <summary>
    /// OrderNo。
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// Status。
    /// </summary>
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// Remark。
    /// </summary>
    [MaxLength(256)]
    public string? Remark { get; set; }
}
