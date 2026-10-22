using Axis.Infrastructure.Components.EventBus;

namespace Axis.Application.Demo.OutboxPing.Events;

/// <summary>
/// Outbox 联调探测事件；不涉及 MES 业务表。
/// </summary>
public sealed class OutboxPingIntegrationEvent : IntegrationEvent
{
    /// <summary>
    /// 探测文案。
    /// </summary>
    public string Message { get; init; } = "ping";
}
