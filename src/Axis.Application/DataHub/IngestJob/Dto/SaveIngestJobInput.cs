using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.DataHub.IngestJob.Dto;

/// <summary>
/// 新增或更新采集任务的输入数据。
/// </summary>
public sealed class SaveIngestJobInput
{
    /// <summary>
    /// 主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// DatasetId。
    /// </summary>
    public long DatasetId { get; set; }

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
    /// Strategy。
    /// </summary>
    public DhUpdateStrategyEnum Strategy { get; set; }

    /// <summary>
    /// CronExpression。
    /// </summary>
    [MaxLength(64)]
    public string? CronExpression { get; set; }

    /// <summary>
    /// BatchSize。
    /// </summary>
    public int BatchSize { get; set; }

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
