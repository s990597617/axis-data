using Axis.Application.DataHub.Common;
using Axis.Application.DataHub.IngestJob.Dto;
using Axis.Infrastructure.Components.Database.Internal;

namespace Axis.Application.DataHub.IngestJob;

/// <summary>
/// 采集任务管理应用服务契约。
/// </summary>
public interface IIngestJobAppService
{
    Task<SqlSugarPagedList<IngestJobDto>> PageAsync(IngestJobPageQuery query, CancellationToken ct = default);

    /// <summary>启用中的采集任务下拉选项。</summary>
    Task<IReadOnlyList<DataHubLookupItemDto>> ListLookupAsync(CancellationToken ct = default);

    Task CreateAsync(SaveIngestJobInput input, CancellationToken ct = default);

    Task UpdateAsync(long id, SaveIngestJobInput input, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 手动触发一次采集执行。
    /// </summary>
    Task<TriggerIngestJobResultDto> TriggerRunAsync(long id, CancellationToken ct = default);
}
