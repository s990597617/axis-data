using Axis.Application.System.Inbox.Events;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Components.EventBus;
using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Notify;
using Yitter.IdGenerator;

namespace Axis.Application.System.Inbox.EventHandlers;

/// <summary>
/// 处理收件箱投递事件：按策略写入 <c>sys_inbox</c> 并按需推送未读。
/// </summary>
/// <remarks>
/// 跨模块写入的唯一落点；不负责撤回（见 <see cref="InboxRecallEventHandler"/>）。
/// </remarks>
public sealed class InboxDeliverEventHandler(
    ISysInboxRepository inboxRepository,
    IUserNotifyPusher notifyPusher)
    : AxisEventHandler<InboxDeliverEvent>
{
    /// <summary>
    /// 处理失败时向上抛出，使 WaitForAll 发布方感知落库失败。
    /// </summary>
    protected override bool ShouldRethrow => true;

    /// <summary>
    /// 校验投递策略后，按 Items 或广播载荷写入收件箱，并可选推送未读。
    /// </summary>
    /// <param name="event">投递事件；Mode 不得为 Unknown。</param>
    /// <param name="ct">取消令牌。</param>
    /// <remarks>
    /// Items 非空时走逐条写入；否则要求 Payload 非空。
    /// ReplaceBySource 会先删同来源再插入，并向旧+新收件人推送未读。
    /// </remarks>
    protected override async Task OnHandleAsync(InboxDeliverEvent @event, CancellationToken ct)
    {
        if (@event.Mode is InboxDeliverModeEnum.Unknown)
            throw new BusinessException("收件箱投递策略无效");

        if (@event.Items.Count > 0)
        {
            await WriteItemsAsync(@event.Mode, @event.Items, @event.PushUnread, ct);
            return;
        }

        if (@event.Payload is null)
            throw new BusinessException("收件箱投递载荷不能为空");

        var targets = @event.UserIds.Where(id => id > 0).Distinct().ToList();
        await WriteBroadcastAsync(@event.Mode, targets, @event.Payload, @event.PushUnread, ct);
    }

    /// <summary>
    /// 同内容向多用户广播写入。
    /// </summary>
    private async Task WriteBroadcastAsync(
        InboxDeliverModeEnum mode,
        IReadOnlyList<long> userIds,
        InboxMessagePayload payload,
        bool pushUnread,
        CancellationToken ct)
    {
        IReadOnlyList<long> pushTargets = userIds;

        if (mode == InboxDeliverModeEnum.ReplaceBySource)
        {
            var previousUserIds = pushUnread
                ? await inboxRepository.ListUserIdsBySourceAsync(payload.SourceType, payload.SourceId, ct)
                : [];

            await inboxRepository.DeleteBySourceAsync(payload.SourceType, payload.SourceId, ct);

            if (userIds.Count == 0)
            {
                if (pushUnread && previousUserIds.Count > 0)
                    await PushUnreadAsync(previousUserIds, ct);
                return;
            }

            pushTargets = previousUserIds.Concat(userIds).Where(id => id > 0).Distinct().ToList();
        }
        else if (userIds.Count == 0)
        {
            return;
        }

        await inboxRepository.InsertDeliveriesAsync(BuildRows(userIds, payload, DateTime.Now), ct);

        if (pushUnread)
            await PushUnreadAsync(pushTargets, ct);
    }

    /// <summary>
    /// 按条目写入；ReplaceBySource 时按来源分组先删后插。
    /// </summary>
    private async Task WriteItemsAsync(
        InboxDeliverModeEnum mode,
        IReadOnlyList<InboxDeliverItem> items,
        bool pushUnread,
        CancellationToken ct)
    {
        var validItems = items
            .Where(item => item.UserId > 0 && item.Payload is not null)
            .ToList();
        if (validItems.Count == 0) return;

        var now = DateTime.Now;

        if (mode == InboxDeliverModeEnum.ReplaceBySource)
        {
            foreach (var group in validItems.GroupBy(item => (item.Payload.SourceType, item.Payload.SourceId)))
            {
                var previousUserIds = pushUnread
                    ? await inboxRepository.ListUserIdsBySourceAsync(group.Key.SourceType, group.Key.SourceId, ct)
                    : [];

                await inboxRepository.DeleteBySourceAsync(group.Key.SourceType, group.Key.SourceId, ct);

                var rows = group
                    .Select(item => BuildRow(item.UserId, item.Payload, now))
                    .ToList();
                await inboxRepository.InsertDeliveriesAsync(rows, ct);

                if (!pushUnread) continue;

                var userIds = previousUserIds
                    .Concat(group.Select(item => item.UserId))
                    .Distinct()
                    .ToList();
                await PushUnreadAsync(userIds, ct);
            }

            return;
        }

        var appendRows = validItems
            .Select(item => BuildRow(item.UserId, item.Payload, now))
            .ToList();
        await inboxRepository.InsertDeliveriesAsync(appendRows, ct);

        if (pushUnread)
        {
            var userIds = validItems.Select(item => item.UserId).Distinct().ToList();
            await PushUnreadAsync(userIds, ct);
        }
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

    private static List<SysInbox> BuildRows(
        IReadOnlyList<long> userIds,
        InboxMessagePayload payload,
        DateTime createdTime)
        => userIds.Select(userId => BuildRow(userId, payload, createdTime)).ToList();

    private static SysInbox BuildRow(long userId, InboxMessagePayload payload, DateTime createdTime)
    {
        var summary = string.IsNullOrWhiteSpace(payload.Summary)
            ? TrimSummary(payload.Content)
            : payload.Summary.Trim();

        return new SysInbox
        {
            Id = YitIdHelper.NextId(),
            UserId = userId,
            Title = payload.Title.Trim(),
            Summary = summary,
            Content = payload.Content,
            SourceType = payload.SourceType,
            SourceId = payload.SourceId,
            BizModule = payload.BizModule,
            BizType = payload.BizType,
            BizId = payload.BizId,
            LinkPath = payload.LinkPath,
            ExtraJson = payload.ExtraJson,
            SenderId = payload.SenderId,
            SenderName = payload.SenderName,
            IsRead = false,
            Priority = payload.Priority,
            CreatedTime = createdTime
        };
    }

    private static string TrimSummary(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return string.Empty;
        var text = content.Replace("\r", " ").Replace("\n", " ").Trim();
        return text.Length <= 80 ? text : text[..80] + "…";
    }
}
