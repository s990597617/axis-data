using Axis.Domain.Enums;

namespace Axis.Application.DataHub.IngestJob.Dto;

/// <summary>
/// 手动触发采集任务后的运行结果摘要。
/// </summary>
public sealed class TriggerIngestJobResultDto
{
    /// <summary>
    /// 本次生成的采集运行 Id。
    /// </summary>
    public long RunId { get; set; }

    /// <summary>
    /// 采集运行编码。
    /// </summary>
    public string RunCode { get; set; } = null!;

    /// <summary>
    /// 运行状态。
    /// </summary>
    public DhRunStatusEnum RunStatus { get; set; }

    /// <summary>
    /// 本次读取行数（受批大小上限约束）。
    /// </summary>
    public long RowCountRead { get; set; }

    /// <summary>
    /// 写入/变更行数；Phase 1 未落 ODS 时为 0。
    /// </summary>
    public long RowCountChanged { get; set; }

    /// <summary>
    /// 增量水位线；非增量策略时为空。
    /// </summary>
    public string? Watermark { get; set; }

    /// <summary>
    /// 失败时的错误摘要。
    /// </summary>
    public string? ErrorMessage { get; set; }
}
