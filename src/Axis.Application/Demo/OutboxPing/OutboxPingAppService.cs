using Axis.Application.Demo.OutboxPing.Dto;
using Axis.Application.Demo.OutboxPing.Events;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.EventBus.Outbox;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.Demo.OutboxPing;

/// <summary>
/// Outbox 联调探测服务。
/// </summary>
public sealed class OutboxPingAppService(
    IOutbox outbox,
    ISysOutboxRepository outboxRepository)
    : IOutboxPingAppService, IAutoInject
{
    /// <summary>
    /// 入队探测事件。
    /// </summary>
    public async Task<OutboxPingDto> PingAsync(OutboxPingInput input, CancellationToken ct = default)
    {
        var message = string.IsNullOrWhiteSpace(input.Message) ? "ping" : input.Message.Trim();
        var eventId = await outbox.EnqueueAsync(new OutboxPingIntegrationEvent { Message = message }, ct);
        if (input.ThrowAfterEnqueue)
            throw new BusinessException("联调回滚：Outbox 行不应落库");

        return new OutboxPingDto { EventId = eventId, Status = OutboxStatusEnum.Pending };
    }

    /// <summary>
    /// 查询探测事件状态。
    /// </summary>
    public async Task<OutboxPingDto> GetAsync(long eventId, CancellationToken ct = default)
    {
        var row = await outboxRepository.FindByEventIdAsync(eventId, ct)
                  ?? throw new BusinessException("Outbox 记录不存在");
        return new OutboxPingDto
        {
            EventId = row.EventId,
            Status = row.Status,
            LastError = row.LastError
        };
    }
}
