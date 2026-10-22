using Axis.Domain.Enums;
using Axis.Infrastructure.Components.EventBus;

namespace Axis.Application.System.Inbox.Events;

/// <summary>
/// 跨模块按来源撤回收件箱记录的集成事件。
/// </summary>
/// <remarks>
/// 典型场景：公告下架、业务消息撤回、报警关闭。
/// 由 <c>InboxRecallEventHandler</c> 删除 <c>sys_inbox</c> 中对应来源行，
/// 并按需向原收件人推送最新未读数。禁止业务方直接删表。
/// </remarks>
public sealed class InboxRecallEvent : AxisEvent
{
    /// <summary>
    /// 消息来源类型，与写入时的 SourceType 一致。
    /// </summary>
    public InboxSourceTypeEnum SourceType { get; init; }

    /// <summary>
    /// 来源业务主键（如公告 Id）；与 SourceType 组成撤回键。
    /// </summary>
    public long SourceId { get; init; }

    /// <summary>
    /// 撤回完成后是否向原收件人推送未读统计。
    /// </summary>
    /// <remarks>
    /// 默认为 true。Handler 会在删除前查出收件人再推送。
    /// </remarks>
    public bool PushUnread { get; init; } = true;
}
