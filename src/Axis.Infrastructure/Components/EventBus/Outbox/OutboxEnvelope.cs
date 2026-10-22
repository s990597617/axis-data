namespace Axis.Infrastructure.Components.EventBus.Outbox;

/// <summary>
/// Outbox 投递信封；传输层与 Broker 入站共用，便于替换 RabbitMQ。
/// </summary>
public sealed class OutboxEnvelope
{
    /// <summary>
    /// 业务事件 Id。
    /// </summary>
    public long EventId { get; init; }

    /// <summary>
    /// 事件 CLR 全名。
    /// </summary>
    public required string EventType { get; init; }

    /// <summary>
    /// JSON 载荷。
    /// </summary>
    public required string Payload { get; init; }

    /// <summary>
    /// 租户 Id；平台事件可空。
    /// </summary>
    public long? TenantId { get; init; }

    /// <summary>
    /// 追踪号。
    /// </summary>
    public string? CorrelationId { get; init; }
}
