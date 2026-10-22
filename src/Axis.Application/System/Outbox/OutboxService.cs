using System.Text.Json;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.EventBus;
using Axis.Infrastructure.Components.EventBus.Outbox;
using Axis.Infrastructure.Dependency.Annotations;
using Yitter.IdGenerator;

namespace Axis.Application.System.Outbox;

/// <summary>
/// Outbox 入队实现。
/// </summary>
public sealed class OutboxService(
    ISysOutboxRepository repository,
    ICurrentAccount currentAccount)
    : IOutbox, IAutoInject
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// 补齐事件字段后插入 Pending 行。
    /// </summary>
    public async Task<long> EnqueueAsync(IIntegrationEvent @event, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(@event);
        AxisEventEnricher.Enrich(@event, currentAccount);

        var type = @event.GetType();
        var typeName = type.FullName
                       ?? throw new InvalidOperationException("集成事件缺少类型全名");
        var axis = @event as AxisEvent;
        var eventId = axis?.EventId ?? YitIdHelper.NextId();

        await repository.AddPendingAsync(new SysOutbox
        {
            Id = YitIdHelper.NextId(),
            EventId = eventId,
            EventType = typeName,
            Payload = JsonSerializer.Serialize(@event, type, JsonOptions),
            TenantId = axis?.TenantId ?? currentAccount.TenantId,
            CorrelationId = axis?.CorrelationId,
            Status = OutboxStatusEnum.Pending,
            CreatedTime = DateTime.Now
        }, ct);

        return eventId;
    }
}
