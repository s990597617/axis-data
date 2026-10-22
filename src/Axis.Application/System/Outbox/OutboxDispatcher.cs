using Axis.Domain.Data.System.Repository;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.EventBus.Outbox;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Axis.Application.System.Outbox;

/// <summary>
/// Outbox 调度实现。
/// </summary>
public sealed class OutboxDispatcher(
    ISysOutboxRepository repository,
    IEnumerable<IOutboxTransport> transports,
    ITenantScope tenantScope,
    IOptions<AxisOutboxOptions> options)
    : IOutboxDispatcher, IAutoInject
{
    /// <summary>
    /// 拉取到期行并逐条投递。
    /// </summary>
    public async Task<int> DispatchOnceAsync(CancellationToken ct = default)
    {
        var settings = options.Value;
        var transport = ResolveTransport(settings.Transport);
        var due = await repository.ListDueAsync(Math.Max(1, settings.BatchSize), DateTime.Now, ct);
        foreach (var row in due)
            await DispatchRowAsync(row, transport, settings, ct);
        return due.Count;
    }

    private IOutboxTransport ResolveTransport(string name)
    {
        var transport = transports.FirstOrDefault(item =>
            string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
        if (transport is not null)
            return transport;

        throw new InvalidOperationException(
            $"未注册 Outbox 传输 '{name}'。第一期请使用 {OutboxTransportNames.InProcess}；" +
            $"接入 RabbitMQ 时实现 IOutboxTransport 且 Name={OutboxTransportNames.RabbitMq}。");
    }

    private async Task DispatchRowAsync(
        SysOutbox row,
        IOutboxTransport transport,
        AxisOutboxOptions settings,
        CancellationToken ct)
    {
        try
        {
            var envelope = new OutboxEnvelope
            {
                EventId = row.EventId,
                EventType = row.EventType,
                Payload = row.Payload,
                TenantId = row.TenantId,
                CorrelationId = row.CorrelationId
            };
            await tenantScope.RunAsync(
                row.TenantId,
                token => transport.PublishAsync(envelope, token),
                ct);
            await repository.MarkPublishedAsync(row.Id, DateTime.Now, ct);
        }
        catch (Exception ex)
        {
            var retry = row.RetryCount + 1;
            var dead = retry >= settings.MaxRetryCount;
            var status = dead ? OutboxStatusEnum.Dead : OutboxStatusEnum.Failed;
            var next = dead ? (DateTime?)null : DateTime.Now.AddSeconds(Math.Min(300, Math.Pow(2, retry)));
            await repository.MarkFailedAsync(row.Id, status, retry, next, Truncate(ex.Message), ct);
        }
    }

    private static string Truncate(string message)
        => message.Length <= 2000 ? message : message[..2000];
}
