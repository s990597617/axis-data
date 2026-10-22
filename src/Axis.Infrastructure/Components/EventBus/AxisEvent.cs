namespace Axis.Infrastructure.Components.EventBus;

/// <summary>
/// 领域事件基类，提供公共属性默认值。
/// </summary>
/// <remarks>
/// 所有业务事件继承此类即可。存量事件可不赋值新字段；MES 新事件必须带
/// <see cref="EventId"/> 与 <see cref="TenantId"/>（发布门面会在缺省时补齐）。
/// </remarks>
public abstract class AxisEvent : IAxisEvent
{
    /// <summary>
    /// 事件发生时间（服务器本地时间），自动设置为创建时间。
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.Now;

    /// <summary>
    /// 事件来源，默认为事件类型名称。
    /// </summary>
    public string Source { get; init; }

    /// <summary>
    /// 稳定事件 Id（雪花）；幂等键。缺省由发布门面补齐。
    /// </summary>
    public long EventId { get; set; }

    /// <summary>
    /// 所属租户；发布时从当前账户或 <c>ITenantScope</c> 写入。
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    /// 一次用户操作或一次设备上报的追踪号。
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// 操作人用户 Id；无登录上下文时可空。
    /// </summary>
    public long? OperatorId { get; set; }

    /// <summary>
    /// 初始化来源为当前类型名。
    /// </summary>
    protected AxisEvent()
    {
        Source = GetType().Name;
    }
}
