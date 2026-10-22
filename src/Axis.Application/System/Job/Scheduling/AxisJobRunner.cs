using System.Diagnostics;
using System.Text.Json;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Jobs;
using Microsoft.Extensions.Logging;
using Quartz;
using SqlSugar;
using Yitter.IdGenerator;

namespace Axis.Application.System.Job.Scheduling;

/// <summary>
/// 禁止并发的 Quartz 任务包装器。
/// </summary>
[DisallowConcurrentExecution]
public sealed class AxisJobRunner(
    AxisJobCatalog catalog,
    ISqlSugarClient db,
    ITenantScope tenantScope,
    ILogger<AxisJobRunner> logger)
    : IJob
{
    /// <inheritdoc />
    public Task Execute(IJobExecutionContext context)
        => AxisJobRunnerCore.ExecuteAsync(context, catalog, db, tenantScope, logger, context.CancellationToken);
}

/// <summary>
/// 允许并发的 Quartz 任务包装器。
/// </summary>
public sealed class AxisJobConcurrentRunner(
    AxisJobCatalog catalog,
    ISqlSugarClient db,
    ITenantScope tenantScope,
    ILogger<AxisJobConcurrentRunner> logger)
    : IJob
{
    /// <inheritdoc />
    public Task Execute(IJobExecutionContext context)
        => AxisJobRunnerCore.ExecuteAsync(context, catalog, db, tenantScope, logger, context.CancellationToken);
}

internal static class AxisJobRunnerCore
{
    private const int MaxExceptionLength = 4000;

    public static async Task ExecuteAsync(
        IJobExecutionContext context,
        AxisJobCatalog catalog,
        ISqlSugarClient db,
        ITenantScope tenantScope,
        ILogger logger,
        CancellationToken ct)
    {
        var map = context.MergedJobDataMap;
        var jobCode = map.GetString(AxisJobDataKeys.JobCode) ?? context.JobDetail.Key.Name;
        var jobId = map.ContainsKey(AxisJobDataKeys.JobId) ? map.GetLong(AxisJobDataKeys.JobId) : 0L;
        var jobName = map.GetString(AxisJobDataKeys.JobName);
        var parameters = map.GetString(AxisJobDataKeys.Params);
        var isManual = map.ContainsKey(AxisJobDataKeys.IsManual) && map.GetBoolean(AxisJobDataKeys.IsManual);

        var executor = catalog.Resolve(jobCode);
        var started = DateTime.Now;
        var sw = Stopwatch.StartNew();
        string? message = null;
        string? exception = null;
        var status = JobExecStatusEnum.Success;

        try
        {
            if (executor is null)
                throw new InvalidOperationException($"未注册任务执行器：{jobCode}");

            var jobContext = new AxisJobContext
            {
                JobId = jobId,
                JobCode = jobCode,
                JobName = jobName,
                Params = string.IsNullOrWhiteSpace(parameters) ? null : parameters,
                IsManual = isManual
            };
            var tenantId = TryReadTenantId(jobContext.Params);
            if (tenantId.HasValue)
                await tenantScope.RunAsync(tenantId, token => executor.ExecuteAsync(jobContext, token), ct);
            else
                await executor.ExecuteAsync(jobContext, ct);

            message = isManual ? "手动触发执行成功" : "调度执行成功";
        }
        catch (Exception ex)
        {
            status = JobExecStatusEnum.Failed;
            message = "执行失败";
            exception = Truncate(ex.ToString(), MaxExceptionLength);
            logger.LogError(ex, "定时任务执行失败：{JobCode}", jobCode);
        }
        finally
        {
            sw.Stop();
            try
            {
                await db.Insertable(new SysJobLog
                {
                    Id = YitIdHelper.NextId(),
                    JobId = jobId,
                    JobCode = jobCode,
                    JobName = jobName,
                    Status = status,
                    TriggerType = isManual ? JobTriggerTypeEnum.Manual : JobTriggerTypeEnum.Cron,
                    StartedTime = started,
                    EndedTime = DateTime.Now,
                    DurationMs = sw.ElapsedMilliseconds,
                    Message = message,
                    Exception = exception
                }).ExecuteCommandAsync(ct);
            }
            catch (Exception logEx)
            {
                logger.LogError(logEx, "写入定时任务日志失败：{JobCode}", jobCode);
            }
        }
    }

    private static long? TryReadTenantId(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("tenantId", out var camel) && camel.TryGetInt64(out var id))
                return id;
            if (doc.RootElement.TryGetProperty("TenantId", out var pascal) && pascal.TryGetInt64(out id))
                return id;
        }
        catch (JsonException)
        {
            // 参数不是 JSON 时由任务自行处理
        }

        return null;
    }

    private static string Truncate(string value, int max)
        => value.Length <= max ? value : value[..max];
}
