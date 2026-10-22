using Axis.Application.System.Job;
using Axis.Application.System.Job.Scheduling;
using Axis.Infrastructure.Components.EventBus.Outbox;
using Axis.Infrastructure.Jobs;
using Axis.Infrastructure.Options;

namespace Axis.Host.Api.HostedServices;

/// <summary>
/// Host.Api 启动任务注册。
/// </summary>
public static class HostedServiceExtensions
{
    /// <summary>
    /// 注册跨模块启动任务与 Quartz 调度（须在数据库初始化 HostedService 之后调用）。
    /// </summary>
    public static IServiceCollection AddAxisHostedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAxisQuartz(configuration);
        services.AddAxisOutbox(configuration);
        services.AddTransient<AxisJobRunner>();
        services.AddTransient<AxisJobConcurrentRunner>();
        services.AddHostedService<MetaCacheWarmupHostedService>();
        services.AddHostedService<JobScheduleSyncHostedService>();
        services.AddHostedService<OutboxDispatchHostedService>();
        return services;
    }
}

/// <summary>
/// 启动后将 sys_job 同步到 Quartz。
/// </summary>
public sealed class JobScheduleSyncHostedService(
    IServiceProvider services,
    ILogger<JobScheduleSyncHostedService> logger)
    : IHostedService
{
    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = services.CreateAsyncScope();
            var options = scope.ServiceProvider.GetService<Microsoft.Extensions.Options.IOptions<AxisQuartzOptions>>()?.Value;
            if (options is { Enabled: false })
            {
                logger.LogInformation("Quartz 已禁用，跳过任务同步");
                return;
            }

            var jobService = scope.ServiceProvider.GetRequiredService<IJobAppService>();
            // SyncSchedulerAsync 内会先 EnsureCatalogSynced（Attribute → sys_job），再同步 Quartz
            await jobService.SyncSchedulerAsync(cancellationToken);
            logger.LogInformation("定时任务目录与 Quartz 调度器已同步");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "定时任务同步失败（已忽略，不影响启动）");
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
