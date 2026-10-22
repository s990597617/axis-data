using Axis.Application.System.Inbox.Events;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.EventBus;
using Axis.Infrastructure.Notify;

namespace Axis.Application.System.Inbox.EventHandlers;

/// <summary>
/// 处理收件箱撤回事件：按来源删除并按需推送未读。
/// </summary>
/// <remarks>
/// 删除前先列出收件人，避免撤回后铃铛未读数不刷新。
/// </remarks>
public sealed class InboxRecallEventHandler(
    ISysInboxRepository inboxRepository,
    IUserNotifyPusher notifyPusher)
    : AxisEventHandler<InboxRecallEvent>
{
    /// <summary>
    /// 处理失败时向上抛出，使 WaitForAll 发布方感知撤回失败。
    /// </summary>
    protected override bool ShouldRethrow => true;

    /// <summary>
    /// 按 SourceType + SourceId 删除收件箱行，并可选向原收件人推送未读。
    /// </summary>
    /// <param name="event">撤回事件。</param>
    /// <param name="ct">取消令牌。</param>
    protected override async Task OnHandleAsync(InboxRecallEvent @event, CancellationToken ct)
    {
        var affectedUserIds = @event.PushUnread
            ? await inboxRepository.ListUserIdsBySourceAsync(@event.SourceType, @event.SourceId, ct)
            : [];

        await inboxRepository.DeleteBySourceAsync(@event.SourceType, @event.SourceId, ct);

        if (@event.PushUnread && affectedUserIds.Count > 0)
            await PushUnreadAsync(affectedUserIds, ct);
    }

    /// <summary>
    /// 向目标用户分别推送最新未读统计。
    /// </summary>
    private Task PushUnreadAsync(IReadOnlyList<long> userIds, CancellationToken ct)
        => notifyPusher.PushUnreadAsync(userIds, userId => BuildUnreadPayloadAsync(userId, ct), ct);

    /// <summary>
    /// 按用户从收件箱表汇总未读推送载荷。
    /// </summary>
    private async Task<UnreadNotifyPayload> BuildUnreadPayloadAsync(long userId, CancellationToken ct)
    {
        var bySource = await inboxRepository.CountUnreadBySourceAsync(userId, ct);
        var notice = bySource.GetValueOrDefault(InboxSourceTypeEnum.Notice);
        var message = bySource.GetValueOrDefault(InboxSourceTypeEnum.Message);
        return new UnreadNotifyPayload
        {
            NoticeCount = notice,
            MessageCount = message,
            Total = bySource.Values.Sum()
        };
    }
}
