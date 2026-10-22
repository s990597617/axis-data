namespace Axis.Infrastructure.Components.EventBus;

/// <summary>
/// 跨模块 / 跨进程集成事件标记。
/// </summary>
/// <remarks>
/// 必须经 <c>IOutbox.EnqueueAsync</c> 入队，禁止直接 <c>IEventPublisher.PublishAsync</c>。
/// 投递由 Dispatcher 经 <c>IOutboxTransport</c> 完成；默认进程内总线，可替换为 RabbitMQ。
/// </remarks>
public interface IIntegrationEvent : IAxisEvent
{
}
