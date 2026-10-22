using Axis.Application.System.Job.Dto;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.System.Job;

/// <summary>
/// 定时任务应用服务。
/// </summary>
public interface IJobAppService
{
    Task<SqlSugarPagedList<JobDto>> PageAsync(JobPageQuery query, CancellationToken ct = default);

    Task CreateAsync(SaveJobInput input, CancellationToken ct = default);

    Task UpdateAsync(long id, SaveJobInput input, CancellationToken ct = default);

    Task DeleteAsync(long id, CancellationToken ct = default);

    Task ChangeStatusAsync(long id, bool enabled, CancellationToken ct = default);

    Task RunAsync(long id, CancellationToken ct = default);

    Task<SqlSugarPagedList<JobLogDto>> PageLogsAsync(JobLogPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 列出已注册执行器（DI 中的 IAxisJob），供管理端选择。
    /// </summary>
    Task<IReadOnlyList<JobExecutorDto>> ListExecutorsAsync(CancellationToken ct = default);

    /// <summary>
    /// 将带 AxisJobAttribute 的执行器幂等同步到 sys_job（不覆盖运维已改的 Cron/状态/参数）。
    /// </summary>
    Task EnsureCatalogSyncedAsync(CancellationToken ct = default);

    Task SyncSchedulerAsync(CancellationToken ct = default);
}
