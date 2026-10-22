namespace Axis.Infrastructure.Components.EventBus.Outbox;

/// <summary>
/// Broker 入站消费口。
/// </summary>
/// <remarks>
/// 进程内传输不走本接口。RabbitMQ 等消费者收到消息后调用，与出站共用反序列化与 Handler 分发。
/// </remarks>
public interface IIntegrationEventIngress
{
    /// <summary>
    /// 处理从消息队列收到的信封。
    /// </summary>
    /// <param name="envelope">信封。</param>
    /// <param name="ct">取消令牌。</param>
    Task HandleAsync(OutboxEnvelope envelope, CancellationToken ct = default);
}
