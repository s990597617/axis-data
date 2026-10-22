namespace Axis.Infrastructure.Jobs;

/// <summary>
/// 定时任务调度器封装（对 Quartz 的薄包装）。
/// </summary>
public interface IAxisJobScheduler
{
    /// <summary>
    /// 按库表配置全量同步到调度器。
    /// </summary>
    Task SyncAllAsync(IEnumerable<AxisJobScheduleDescriptor> jobs, CancellationToken ct = default);

    /// <summary>
    /// 新增或更新单个任务调度。
    /// </summary>
    Task ScheduleOrUpdateAsync(AxisJobScheduleDescriptor job, CancellationToken ct = default);

    /// <summary>
    /// 移除调度。
    /// </summary>
    Task UnscheduleAsync(string jobCode, CancellationToken ct = default);

    /// <summary>
    /// 暂停。
    /// </summary>
    Task PauseAsync(string jobCode, CancellationToken ct = default);

    /// <summary>
    /// 恢复。
    /// </summary>
    Task ResumeAsync(string jobCode, CancellationToken ct = default);

    /// <summary>
    /// 立即触发一次。
    /// </summary>
    Task TriggerAsync(string jobCode, bool manual = true, CancellationToken ct = default);

    /// <summary>
    /// 下次触发时间（本地时区）。
    /// </summary>
    Task<DateTime?> GetNextFireTimeAsync(string jobCode, CancellationToken ct = default);

    /// <summary>
    /// 校验 Cron 是否合法。
    /// </summary>
    bool IsValidCron(string cron);
}
