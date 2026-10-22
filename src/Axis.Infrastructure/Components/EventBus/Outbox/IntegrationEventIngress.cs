namespace Axis.Infrastructure.Components.EventBus.Outbox;

/// <summary>
/// Broker 入站默认实现，与进程内投递共用执行器。
/// </summary>
public sealed class IntegrationEventIngress(IOutboxDeliveryExecutor executor)
    : IIntegrationEventIngress, IAutoInject
{
    /// <summary>
    /// 处理队列消息。
    /// </summary>
    public Task HandleAsync(OutboxEnvelope envelope, CancellationToken ct = default)
        => executor.DeliverAsync(envelope, ct);
}
