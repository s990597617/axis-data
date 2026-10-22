using Axis.Application.System.Inbox.Dto;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Notify;

namespace Axis.Application.System.Inbox;

/// <summary>
/// 用户收件箱应用服务：基于 <c>sys_inbox</c> 的查询与已读。
/// </summary>
public sealed class InboxAppService(
    ISysInboxRepository inboxRepository,
    ICurrentAccount currentAccount,
    IUserNotifyPusher notifyPusher)
    : IInboxAppService, IAutoInject
{
    /// <summary>
    /// 汇总当前用户未读并按来源拆分。
    /// </summary>
    public async Task<UnreadCountDto> GetUnreadCountAsync(CancellationToken ct = default)
    {
        var payload = await GetUnreadPayloadAsync(currentAccount.Id, ct);
        return new UnreadCountDto
        {
            Total = payload.Total,
            NoticeCount = payload.NoticeCount,
            MessageCount = payload.MessageCount,
            OtherCount = Math.Max(0, payload.Total - payload.NoticeCount - payload.MessageCount)
        };
    }

    /// <summary>
    /// 从收件箱表统计未读（单次分组查询）。
    /// </summary>
    public async Task<UnreadNotifyPayload> GetUnreadPayloadAsync(long userId, CancellationToken ct = default)
    {
        var bySource = await inboxRepository.CountUnreadBySourceAsync(userId, ct);
        var notice = bySource.GetValueOrDefault(InboxSourceTypeEnum.Notice);
        var message = bySource.GetValueOrDefault(InboxSourceTypeEnum.Message);
        var total = bySource.Values.Sum();

        return new UnreadNotifyPayload
        {
            NoticeCount = notice,
            MessageCount = message,
            Total = total
        };
    }

    /// <summary>
    /// 按当前用户分页查询收件箱。
    /// </summary>
    public async Task<SqlSugarPagedList<InboxItemDto>> PageAsync(InboxPageQuery query, CancellationToken ct = default)
    {
        var page = await inboxRepository.PageByUserAsync(
            currentAccount.Id,
            query.Page,
            query.PageSize,
            query.Keyword,
            query.SourceType,
            query.IsRead,
            ct);

        return new SqlSugarPagedList<InboxItemDto>
        {
            Page = page.Page,
            PageSize = page.PageSize,
            Total = page.Total,
            Items = page.Items.Select(MapListItem).ToList()
        };
    }

    /// <summary>
    /// 获取本人收件箱详情。
    /// </summary>
    public async Task<InboxItemDto?> GetAsync(long id, CancellationToken ct = default)
    {
        var entity = await inboxRepository.GetOwnedAsync(id, currentAccount.Id, ct);
        return entity is null ? null : MapDetail(entity);
    }

    /// <summary>
    /// 标记已读并推送未读数。
    /// </summary>
    public async Task MarkReadAsync(long id, CancellationToken ct = default)
    {
        var entity = await inboxRepository.GetOwnedAsync(id, currentAccount.Id, ct)
                     ?? throw new BusinessException("消息不存在");

        await inboxRepository.MarkReadAsync(entity.Id, currentAccount.Id, ct);

        var payload = await GetUnreadPayloadAsync(currentAccount.Id, ct);
        await notifyPusher.PushUnreadAsync(currentAccount.Id, payload, ct);
    }

    /// <summary>
    /// 全部已读并推送。
    /// </summary>
    public async Task MarkAllReadAsync(CancellationToken ct = default)
    {
        await inboxRepository.MarkAllReadAsync(currentAccount.Id, ct);
        var payload = await GetUnreadPayloadAsync(currentAccount.Id, ct);
        await notifyPusher.PushUnreadAsync(currentAccount.Id, payload, ct);
    }

    /// <summary>
    /// 按来源标记当前用户已读并推送。
    /// </summary>
    public async Task MarkReadBySourceAsync(InboxSourceTypeEnum sourceType, long sourceId, CancellationToken ct = default)
    {
        await inboxRepository.MarkReadBySourceAsync(currentAccount.Id, sourceType, sourceId, ct);
        var payload = await GetUnreadPayloadAsync(currentAccount.Id, ct);
        await notifyPusher.PushUnreadAsync(currentAccount.Id, payload, ct);
    }

    private static InboxItemDto MapListItem(SysInbox item)
        => new()
        {
            Id = item.Id,
            SourceType = item.SourceType,
            SourceId = item.SourceId,
            Title = item.Title,
            Summary = item.Summary,
            IsRead = item.IsRead,
            CreatedTime = item.CreatedTime,
            SenderName = item.SenderName,
            BizModule = item.BizModule,
            BizType = item.BizType,
            BizId = item.BizId,
            LinkPath = item.LinkPath,
            ResolvedLink = ResolveLink(item.LinkPath, item.BizId),
            ExtraJson = item.ExtraJson,
            Priority = item.Priority
        };

    private static InboxItemDto MapDetail(SysInbox item)
    {
        var dto = MapListItem(item);
        dto.Content = item.Content;
        dto.Summary = string.IsNullOrWhiteSpace(item.Content) ? item.Summary : item.Content;
        return dto;
    }

    private static string? ResolveLink(string? linkPath, long? bizId)
    {
        if (string.IsNullOrWhiteSpace(linkPath)) return null;
        if (bizId is null or <= 0) return linkPath;
        return linkPath.Replace("{bizId}", bizId.Value.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
