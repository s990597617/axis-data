namespace Axis.Infrastructure.Components.EventBus.Outbox;

/// <summary>
/// Outbox 出站传输。
/// </summary>
/// <remarks>
/// 默认 <c>InProcess</c> 走进程内总线。RabbitMQ 实现应把 <see cref="OutboxEnvelope"/>
/// 发到交换机，消费端再调 <see cref="IIntegrationEventIngress"/>。
/// </remarks>
public interface IOutboxTransport
{
    /// <summary>
    /// 与 <c>Outbox:Transport</c> 配置对齐的名称。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 投递一条已提交的 Outbox 消息。
    /// </summary>
    /// <param name="envelope">信封。</param>
    /// <param name="ct">取消令牌。</param>
    Task PublishAsync(OutboxEnvelope envelope, CancellationToken ct = default);
}
