using Axis.Application.DataHub.IngestJob.Dto;

namespace Axis.Application.DataHub.IngestJob;

/// <summary>
/// 采集任务执行器：创建运行记录并在源端完成受控读取。
/// </summary>
public interface IIngestJobRunner
{
    /// <summary>
    /// 对指定采集任务执行一次手动采集（同步、受 DefaultBatchSize 约束）。
    /// </summary>
    Task<TriggerIngestJobResultDto> ExecuteAsync(long jobId, CancellationToken ct = default);
}
