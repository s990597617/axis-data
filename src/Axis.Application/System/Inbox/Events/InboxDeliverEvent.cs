using Axis.Domain.Enums;
using Axis.Infrastructure.Components.EventBus;

namespace Axis.Application.System.Inbox.Events;

/// <summary>
/// 跨模块请求向用户收件箱写入消息的集成事件。
/// </summary>
/// <remarks>
/// <para>
/// 任意业务模块（公告、站内信、报警等）只应发布本事件，由
/// <c>InboxDeliverEventHandler</c> 写入 <c>sys_inbox</c> 并按需推送未读。
/// 禁止业务方直接注入收件箱仓储或绕过事件写库。
/// </para>
/// <para>
/// 同内容广播：填写 <see cref="Payload"/> 与 <see cref="UserIds"/>。
/// 每人独立来源或内容：填写 <see cref="Items"/>
/// （通常配合 <see cref="InboxDeliverModeEnum.Append"/>）。
/// 二者勿同时混用；Handler 优先处理 <see cref="Items"/>。
/// </para>
/// </remarks>
public sealed class InboxDeliverEvent : AxisEvent
{
    /// <summary>
    /// 写入策略；决定是否先按来源撤回再插入。
    /// </summary>
    /// <remarks>
    /// 取自 <see cref="InboxDeliverModeEnum"/>；不得使用 <see cref="InboxDeliverModeEnum.Unknown"/>。
    /// </remarks>
    public InboxDeliverModeEnum Mode { get; init; }

    /// <summary>
    /// 广播收件人用户 Id 列表；与 <see cref="Payload"/> 成对使用。
    /// </summary>
    /// <remarks>
    /// 仅保留大于 0 的 Id；空列表在 ReplaceBySource 时相当于只撤回不写入。
    /// 使用 <see cref="Items"/> 时本集合应保持为空。
    /// </remarks>
    public IReadOnlyList<long> UserIds { get; init; } = [];

    /// <summary>
    /// 广播消息内容载荷；所有 <see cref="UserIds"/> 共享同一份内容与来源。
    /// </summary>
    /// <remarks>
    /// 使用广播模式时必填；使用 <see cref="Items"/> 时可为空。
    /// </remarks>
    public InboxMessagePayload? Payload { get; init; }

    /// <summary>
    /// 逐条投递项；每人可有独立载荷与 SourceId。
    /// </summary>
    /// <remarks>
    /// 与 <see cref="UserIds"/> / <see cref="Payload"/> 二选一。
    /// 非空时 Handler 忽略广播字段。
    /// </remarks>
    public IReadOnlyList<InboxDeliverItem> Items { get; init; } = [];

    /// <summary>
    /// 写入完成后是否向受影响用户推送未读统计。
    /// </summary>
    /// <remarks>
    /// 默认为 true。批量静默补数据时可显式设为 false。
    /// </remarks>
    public bool PushUnread { get; init; } = true;
}
