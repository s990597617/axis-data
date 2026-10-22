using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.DataHub.IngestRun.Dto;

/// <summary>
/// 新增或更新采集运行的输入数据。
/// </summary>
public sealed class SaveIngestRunInput
{
    /// <summary>
    /// 主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// JobId。
    /// </summary>
    public long JobId { get; set; }

    /// <summary>
    /// Name。
    /// </summary>
    [Required, MaxLength(128)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// Code。
    /// </summary>
    [Required, MaxLength(64)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// RunStatus。
    /// </summary>
    public DhRunStatusEnum RunStatus { get; set; }

    /// <summary>
    /// StartedTime。
    /// </summary>
    public DateTime? StartedTime { get; set; }

    /// <summary>
    /// FinishedTime。
    /// </summary>
    public DateTime? FinishedTime { get; set; }

    /// <summary>
    /// RowCountRead。
    /// </summary>
    public long RowCountRead { get; set; }

    /// <summary>
    /// RowCountChanged。
    /// </summary>
    public long RowCountChanged { get; set; }

    /// <summary>
    /// ErrorMessage。
    /// </summary>
    [MaxLength(1024)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Watermark。
    /// </summary>
    [MaxLength(512)]
    public string? Watermark { get; set; }

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
