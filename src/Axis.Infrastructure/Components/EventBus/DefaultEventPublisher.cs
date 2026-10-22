using Axis.Infrastructure.Author;
using FastEndpoints;
using Microsoft.AspNetCore.Http;

namespace Axis.Infrastructure.Components.EventBus;

/// <summary>
/// 默认事件发布者实现，委托给 FastEndpoints Event Bus。
/// </summary>
[Injection(ServiceLifetime.Singleton)]
public sealed class DefaultEventPublisher(IHttpContextAccessor? accessor = null)
    : IEventPublisher, IAutoInject
{
    /// <summary>
    /// 发布并等待全部 Handler。
    /// </summary>
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IAxisEvent
    {
        AxisEventEnricher.Enrich(@event, ResolveAccount());
        return @event.PublishAsync(Mode.WaitForAll, ct);
    }

    /// <summary>
    /// 发布且不等待 Handler。
    /// </summary>
    public Task PublishFireAndForgetAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IAxisEvent
    {
        AxisEventEnricher.Enrich(@event, ResolveAccount());
        return @event.PublishAsync(Mode.WaitForNone, ct);
    }

    private ICurrentAccount? ResolveAccount()
        => accessor?.HttpContext?.RequestServices.GetService<ICurrentAccount>();
}
