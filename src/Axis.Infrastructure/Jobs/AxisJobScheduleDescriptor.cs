namespace Axis.Infrastructure.Jobs;

/// <summary>
/// 调度描述（由业务层从 sys_job 映射而来）。
/// </summary>
public sealed record AxisJobScheduleDescriptor(
    long JobId,
    string JobCode,
    string JobName,
    string Cron,
    bool Concurrent,
    string Params,
    bool Enabled);
