using Axis.Domain.Enums;

namespace Axis.Application.DataHub.IngestJob.Dto;

/// <summary>
/// 采集任务列表项及编辑回显数据。
/// </summary>
public sealed class IngestJobDto
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// DatasetId。
    /// </summary>
    public long DatasetId { get; set; }

    /// <summary>
    /// Name。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Code。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// Strategy。
    /// </summary>
    public DhUpdateStrategyEnum Strategy { get; set; }

    /// <summary>
    /// CronExpression。
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// BatchSize。
    /// </summary>
    public int BatchSize { get; set; }

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
