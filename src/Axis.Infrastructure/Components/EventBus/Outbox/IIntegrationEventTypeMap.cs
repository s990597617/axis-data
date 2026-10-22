namespace Axis.Infrastructure.Components.EventBus.Outbox;

/// <summary>
/// 集成事件类型目录，按 CLR 全名还原类型。
/// </summary>
public interface IIntegrationEventTypeMap
{
    /// <summary>
    /// 解析事件类型；未知时返回 <c>null</c>。
    /// </summary>
    /// <param name="eventType">类型全名。</param>
    /// <returns>CLR 类型。</returns>
    Type? Resolve(string eventType);
}
