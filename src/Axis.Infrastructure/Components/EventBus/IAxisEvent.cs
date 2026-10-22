using FastEndpoints;

namespace Axis.Infrastructure.Components.EventBus;

/// <summary>
/// Axis 领域事件标记接口。
/// <para>
/// 继承 FastEndpoints <see cref="IEvent"/>，所有领域事件均实现此接口。
/// 事件发布后由对应的 <see cref="IEventHandler{TEvent}"/> 自动订阅处理。
/// </para>
/// <para>用法：</para>
/// <code>
/// // 定义事件
/// public class UserCreatedEvent : IAxisEvent { ... }
///
/// // 发布事件（任意位置）
/// await new UserCreatedEvent { UserId = 123 }.PublishAsync(Mode.WaitForNone);
///
/// // 处理事件
/// public class SendWelcomeEmail : AxisEventHandler&lt;UserCreatedEvent&gt; { ... }
/// </code>
/// </summary>
public interface IAxisEvent : IEvent
{
    /// <summary>
    /// 事件发生时间（服务器本地时间）
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// 事件来源（类名或模块名，便于追踪）
    /// </summary>
    string Source { get; }
}
