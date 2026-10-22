using System.Text.Json;
using FastEndpoints;

namespace Axis.Infrastructure.Components.EventBus.Outbox;

/// <summary>
/// 默认投递执行器。
/// </summary>
public sealed class OutboxDeliveryExecutor(IIntegrationEventTypeMap typeMap)
    : IOutboxDeliveryExecutor, IAutoInject
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// 按事件类型还原实例并发布。
    /// </summary>
    public async Task DeliverAsync(OutboxEnvelope envelope, CancellationToken ct = default)
    {
        var type = typeMap.Resolve(envelope.EventType)
                   ?? throw new InvalidOperationException($"未知集成事件类型：{envelope.EventType}");
        var evt = JsonSerializer.Deserialize(envelope.Payload, type, JsonOptions) as IAxisEvent
                  ?? throw new InvalidOperationException("集成事件载荷为空");
        await evt.PublishAsync(Mode.WaitForAll, ct);
    }
}
