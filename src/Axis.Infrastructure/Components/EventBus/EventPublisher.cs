using FastEndpoints;

namespace Axis.Infrastructure.Components.EventBus;

/// <summary>
/// 事件发布工具类。
/// <para>
/// 对 FastEndpoints <see cref="IEvent.PublishAsync"/> 进行统一封装，
/// 提供在 Endpoint 之外（AppService / 领域服务 / 后台任务）发布事件的能力。
/// </para>
/// <para>
/// 在 Endpoint 内部可直接调用 <c>PublishAsync()</c>（FastEndpoints 内置），
/// 在其他位置使用本类的静态方法或注入 <see cref="IEventPublisher"/> 接口。
/// </para>
/// </summary>
public static class EventPublisher
{
    /// <summary>
    /// 发布事件（等待所有订阅者完成）
    /// </summary>
    public static Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IAxisEvent
    {
        AxisEventEnricher.Enrich(@event, account: null);
        return @event.PublishAsync(Mode.WaitForAll, ct);
    }

    /// <summary>
    /// 发布事件（Fire-and-Forget，不等待订阅者）
    /// </summary>
    public static Task PublishFireAndForgetAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IAxisEvent
    {
        AxisEventEnricher.Enrich(@event, account: null);
        return @event.PublishAsync(Mode.WaitForNone, ct);
    }

    /// <summary>
    /// 发布事件（等待任意一个订阅者完成即返回）
    /// </summary>
    public static Task PublishWaitForAnyAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IAxisEvent
    {
        AxisEventEnricher.Enrich(@event, account: null);
        return @event.PublishAsync(Mode.WaitForAny, ct);
    }
}
