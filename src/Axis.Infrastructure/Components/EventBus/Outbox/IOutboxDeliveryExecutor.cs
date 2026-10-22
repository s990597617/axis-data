namespace Axis.Infrastructure.Components.EventBus.Outbox;

/// <summary>
/// 将信封反序列化后发布到进程内 Event Bus，供出站与 Broker 入站复用。
/// </summary>
public interface IOutboxDeliveryExecutor
{
    /// <summary>
    /// 反序列化并等待全部 Handler。
    /// </summary>
    /// <param name="envelope">信封。</param>
    /// <param name="ct">取消令牌。</param>
    Task DeliverAsync(OutboxEnvelope envelope, CancellationToken ct = default);
}
