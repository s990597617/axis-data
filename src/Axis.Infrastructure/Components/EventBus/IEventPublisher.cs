namespace Axis.Infrastructure.Components.EventBus;

/// <summary>
/// 事件发布者接口（可注入版本）。
/// <para>
/// 适用于需要 Mock / 测试的场景，或偏好构造函数注入的编码风格。
/// 默认实现直接委托给 FastEndpoints 的 <c>IEvent.PublishAsync()</c>。
/// </para>
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// 发布事件并等待所有订阅者处理完成
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IAxisEvent;

    /// <summary>
    /// 发布事件，不等待（Fire-and-Forget）
    /// </summary>
    Task PublishFireAndForgetAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : IAxisEvent;
}
