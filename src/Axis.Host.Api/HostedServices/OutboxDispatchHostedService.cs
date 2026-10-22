using Axis.Infrastructure.Components.EventBus.Outbox;
using Axis.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Axis.Host.Api.HostedServices;

/// <summary>
/// 周期扫描 Outbox 并投递。
/// </summary>
public sealed class OutboxDispatchHostedService(
    IServiceScopeFactory scopes,
    IOptions<AxisOutboxOptions> options,
    ILogger<OutboxDispatchHostedService> logger)
    : BackgroundService
{
    /// <summary>
    /// 按配置间隔拉取并投递。
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            logger.LogInformation("Outbox Dispatcher 已禁用");
            return;
        }

        var delay = TimeSpan.FromMilliseconds(Math.Max(500, settings.PollIntervalMs));
        logger.LogInformation("Outbox Dispatcher 已启动，传输={Transport}，间隔={Interval}ms",
            settings.Transport, delay.TotalMilliseconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxDispatcher>();
                await dispatcher.DispatchOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox 本轮投递失败");
            }

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
