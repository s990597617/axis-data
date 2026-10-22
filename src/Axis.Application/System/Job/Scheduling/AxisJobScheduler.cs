using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Jobs;
using Axis.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Impl.Matchers;

namespace Axis.Application.System.Job.Scheduling;

/// <summary>
/// Quartz 调度封装实现。
/// </summary>
[Injection(ServiceLifetime.Singleton, InjectAs.SelfWithInterfaces)]
public sealed class AxisJobScheduler(
    ISchedulerFactory schedulerFactory,
    IOptions<AxisQuartzOptions> options)
    : IAxisJobScheduler, IAutoInject
{
    /// <inheritdoc />
    public async Task SyncAllAsync(IEnumerable<AxisJobScheduleDescriptor> jobs, CancellationToken ct = default)
    {
        if (!options.Value.Enabled) return;

        var list = jobs.ToList();
        var scheduler = await schedulerFactory.GetScheduler(ct);
        var existing = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(AxisJobDataKeys.Group), ct);

        foreach (var key in existing)
        {
            if (list.All(item => !string.Equals(item.JobCode, key.Name, StringComparison.OrdinalIgnoreCase)))
                await scheduler.DeleteJob(key, ct);
        }

        foreach (var job in list)
            await ScheduleOrUpdateAsync(job, ct);
    }

    /// <inheritdoc />
    public async Task ScheduleOrUpdateAsync(AxisJobScheduleDescriptor job, CancellationToken ct = default)
    {
        if (!options.Value.Enabled) return;
        if (!IsValidCron(job.Cron))
            throw new ArgumentException($"非法 Cron 表达式：{job.Cron}");

        var scheduler = await schedulerFactory.GetScheduler(ct);
        var jobKey = CreateJobKey(job.JobCode);
        var triggerKey = CreateTriggerKey(job.JobCode);
        var jobDetail = BuildJobDetail(job);

        if (await scheduler.CheckExists(jobKey, ct))
            await scheduler.AddJob(jobDetail, true, true, ct);
        else
            await scheduler.AddJob(jobDetail, false, true, ct);

        var trigger = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .ForJob(jobKey)
            .WithCronSchedule(job.Cron, x => x.WithMisfireHandlingInstructionDoNothing())
            .Build();

        if (await scheduler.CheckExists(triggerKey, ct))
            await scheduler.RescheduleJob(triggerKey, trigger, ct);
        else
            await scheduler.ScheduleJob(trigger, ct);

        if (!job.Enabled)
            await scheduler.PauseJob(jobKey, ct);
        else
            await scheduler.ResumeJob(jobKey, ct);
    }

    /// <inheritdoc />
    public async Task UnscheduleAsync(string jobCode, CancellationToken ct = default)
    {
        if (!options.Value.Enabled) return;
        var scheduler = await schedulerFactory.GetScheduler(ct);
        await scheduler.DeleteJob(CreateJobKey(jobCode), ct);
    }

    /// <inheritdoc />
    public async Task PauseAsync(string jobCode, CancellationToken ct = default)
    {
        if (!options.Value.Enabled) return;
        var scheduler = await schedulerFactory.GetScheduler(ct);
        await scheduler.PauseJob(CreateJobKey(jobCode), ct);
    }

    /// <inheritdoc />
    public async Task ResumeAsync(string jobCode, CancellationToken ct = default)
    {
        if (!options.Value.Enabled) return;
        var scheduler = await schedulerFactory.GetScheduler(ct);
        await scheduler.ResumeJob(CreateJobKey(jobCode), ct);
    }

    /// <inheritdoc />
    public async Task TriggerAsync(string jobCode, bool manual = true, CancellationToken ct = default)
    {
        if (!options.Value.Enabled) return;
        var scheduler = await schedulerFactory.GetScheduler(ct);
        var data = new JobDataMap { { AxisJobDataKeys.IsManual, manual } };
        await scheduler.TriggerJob(CreateJobKey(jobCode), data, ct);
    }

    /// <inheritdoc />
    public async Task<DateTime?> GetNextFireTimeAsync(string jobCode, CancellationToken ct = default)
    {
        if (!options.Value.Enabled) return null;
        var scheduler = await schedulerFactory.GetScheduler(ct);
        var trigger = await scheduler.GetTrigger(CreateTriggerKey(jobCode), ct);
        var next = trigger?.GetNextFireTimeUtc();
        return next?.LocalDateTime;
    }

    /// <inheritdoc />
    public bool IsValidCron(string cron)
    {
        if (string.IsNullOrWhiteSpace(cron)) return false;
        return CronExpression.IsValidExpression(cron.Trim());
    }

    private static IJobDetail BuildJobDetail(AxisJobScheduleDescriptor job)
    {
        var jobType = job.Concurrent ? typeof(AxisJobConcurrentRunner) : typeof(AxisJobRunner);
        return JobBuilder.Create(jobType)
            .WithIdentity(CreateJobKey(job.JobCode))
            .WithDescription(job.JobName)
            .UsingJobData(AxisJobDataKeys.JobId, job.JobId)
            .UsingJobData(AxisJobDataKeys.JobCode, job.JobCode)
            .UsingJobData(AxisJobDataKeys.JobName, job.JobName ?? string.Empty)
            .UsingJobData(AxisJobDataKeys.Params, job.Params ?? string.Empty)
            .StoreDurably()
            .Build();
    }

    private static JobKey CreateJobKey(string jobCode) => new(jobCode, AxisJobDataKeys.Group);

    private static TriggerKey CreateTriggerKey(string jobCode) => new($"{jobCode}.trigger", AxisJobDataKeys.Group);
}
