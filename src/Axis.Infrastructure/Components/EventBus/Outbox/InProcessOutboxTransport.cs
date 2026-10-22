namespace Axis.Infrastructure.Components.EventBus.Outbox;

/// <summary>
/// 进程内传输：提交后走 FastEndpoints Event Bus。
/// </summary>
public sealed class InProcessOutboxTransport(IOutboxDeliveryExecutor executor)
    : IOutboxTransport, IAutoInject
{
    /// <summary>
    /// 传输名。
    /// </summary>
    public string Name => OutboxTransportNames.InProcess;

    /// <summary>
    /// 投递到进程内 Handler。
    /// </summary>
    public Task PublishAsync(OutboxEnvelope envelope, CancellationToken ct = default)
        => executor.DeliverAsync(envelope, ct);
}
