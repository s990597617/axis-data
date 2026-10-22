using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.DataHub.IngestRun.Dto;

/// <summary>
/// 创建或更新采集运行的请求数据。
/// </summary>
public sealed class SaveIngestRunInput
{
    public long Id { get; set; }

    /// <summary>采集任务 Id（必选）。</summary>
    public long JobId { get; set; }

    /// <summary>名称；为空时按任务自动生成。</summary>
    [MaxLength(128)]
    public string? Name { get; set; }

    /// <summary>编码；为空时按任务自动生成。</summary>
    [MaxLength(64)]
    public string? Code { get; set; }

    public DhRunStatusEnum RunStatus { get; set; } = DhRunStatusEnum.Pending;

    public DateTime? StartedTime { get; set; }

    public DateTime? FinishedTime { get; set; }

    public long RowCountRead { get; set; }

    public long RowCountChanged { get; set; }

    [MaxLength(1024)]
    public string? ErrorMessage { get; set; }

    [MaxLength(512)]
    public string? Watermark { get; set; }

    public int OrderNo { get; set; } = 100;

    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    [MaxLength(256)]
    public string? Remark { get; set; }
}
