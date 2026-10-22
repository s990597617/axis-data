namespace Axis.Infrastructure.Components.EventBus.Events;

/// <summary>
/// 实体变更类型
/// </summary>
public enum ChangeType
{
    Created,
    Updated,
    Deleted
}

/// <summary>
/// 通用实体变更事件。
/// <para>适用于需要监听数据变化并触发联动的场景（如缓存刷新、同步通知等）。</para>
/// </summary>
public class EntityChangedEvent : AxisEvent
{
    /// <summary>
    /// 实体类型名称（如 "SysUser"、"SysRole"）
    /// </summary>
    public string EntityType { get; init; }

    /// <summary>
    /// 实体 ID
    /// </summary>
    public long EntityId { get; init; }

    /// <summary>
    /// 变更类型
    /// </summary>
    public ChangeType ChangeType { get; init; }

    /// <summary>
    /// 附加数据（可选，如变更字段快照）
    /// </summary>
    public object Payload { get; init; }
}
