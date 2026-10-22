namespace Axis.Infrastructure.Components.EventBus;

/// <summary>
/// 集成事件基类；跨模块副作用与出站消息必须继承本类并走 Outbox。
/// </summary>
public abstract class IntegrationEvent : AxisEvent, IIntegrationEvent
{
}
