using Axis.Application.Demo.OutboxPing.Events;
using Axis.Infrastructure.Components.EventBus;
using Microsoft.Extensions.Logging;

namespace Axis.Application.Demo.OutboxPing.EventHandlers;

/// <summary>
/// 记录 Outbox 探测事件，用于确认 Dispatcher 已投递。
/// </summary>
public sealed class OutboxPingEventHandler(ILogger<OutboxPingEventHandler> logger)
    : AxisEventHandler<OutboxPingIntegrationEvent>
{
    /// <summary>
    /// 写一条信息日志，不访问业务表。
    /// </summary>
    protected override Task OnHandleAsync(OutboxPingIntegrationEvent @event, CancellationToken ct)
    {
        logger.LogInformation(
            "Outbox 探测已投递：EventId={EventId}, TenantId={TenantId}, Message={Message}",
            @event.EventId,
            @event.TenantId,
            @event.Message);
        return Task.CompletedTask;
    }
}
