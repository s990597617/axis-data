using Axis.Application.System.Inbox.Events;
using Axis.Application.System.Notice.Dto;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.EventBus;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using Mapster;
using Yitter.IdGenerator;

namespace Axis.Application.System.Notice;

/// <summary>
/// 通知公告应用服务：草稿维护与发布；收件箱投递通过事件交给消息模块。
/// </summary>
public sealed class NoticeAppService(
    ISysNoticeRepository noticeRepository,
    ICurrentAccount currentAccount)
    : INoticeAppService, IAutoInject
{
    /// <summary>
    /// 管理端分页查询通知公告。
    /// </summary>
    public async Task<SqlSugarPagedList<NoticeDto>> PageAsync(NoticePageQuery query, CancellationToken ct = default)
    {
        var page = await noticeRepository.PageAsync(
            query.Page,
            query.PageSize,
            query.Keyword,
            query.NoticeType,
            query.Status,
            ct);
        return page.Adapt<SqlSugarPagedList<NoticeDto>>();
    }

    /// <summary>
    /// 新增公告；启用则发布收件箱事件。
    /// </summary>
    public async Task CreateAsync(SaveNoticeInput input, CancellationToken ct = default)
    {
        var entity = new SysNotice
        {
            Id = YitIdHelper.NextId(),
            CreatedBy = currentAccount.Id,
            Creator = currentAccount.RealName,
            CreatedTime = DateTime.Now
        };
        ApplyInput(entity, input);
        if (entity.Status == StatusEnum.Enable)
            entity.PublishTime = DateTime.Now;

        await noticeRepository.InsertAsync(entity);

        if (entity.Status == StatusEnum.Enable)
            await PublishInboxEventAsync(entity, ct);
    }

    /// <summary>
    /// 更新公告；启用时重投递，停用时撤回。
    /// </summary>
    public async Task UpdateAsync(long id, SaveNoticeInput input, CancellationToken ct = default)
    {
        var entity = await noticeRepository.GetByIdAsync(id)
                     ?? throw new BusinessException("通知公告不存在");

        var wasPublished = entity.Status == StatusEnum.Enable;
        ApplyInput(entity, input);
        entity.ModifiedBy = currentAccount.Id;
        entity.Modifier = currentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;
        if (!wasPublished && entity.Status == StatusEnum.Enable)
            entity.PublishTime = DateTime.Now;

        await noticeRepository.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);

        if (entity.Status == StatusEnum.Enable)
            await PublishInboxEventAsync(entity, ct);
        else
            await RecallInboxEventAsync(entity.Id, ct);
    }

    /// <summary>
    /// 删除公告并撤回收件箱。
    /// </summary>
    public async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        _ = await noticeRepository.GetByIdAsync(id)
            ?? throw new BusinessException("通知公告不存在");

        await RecallInboxEventAsync(id, ct);
        await noticeRepository.DeleteByIdAsync(id);
    }

    /// <summary>
    /// 发布或下架。
    /// </summary>
    public async Task PublishAsync(long id, bool publish, CancellationToken ct = default)
    {
        var entity = await noticeRepository.GetByIdAsync(id)
                     ?? throw new BusinessException("通知公告不存在");

        entity.Status = publish ? StatusEnum.Enable : StatusEnum.Disable;
        if (publish)
            entity.PublishTime = DateTime.Now;
        entity.ModifiedBy = currentAccount.Id;
        entity.Modifier = currentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;
        // 列更新豁免乐观锁：发布/下架仅改状态列。
        await noticeRepository.AsUpdateable(entity)
            .UpdateColumns(item => new
            {
                item.Status,
                item.PublishTime,
                item.ModifiedBy,
                item.Modifier,
                item.ModifiedTime
            })
            .ExecuteCommandAsync(ct);

        if (publish)
            await PublishInboxEventAsync(entity, ct);
        else
            await RecallInboxEventAsync(entity.Id, ct);
    }

    /// <summary>
    /// 解析目标用户后发布投递事件（由 Inbox Handler 落库+推送）。
    /// </summary>
    private async Task PublishInboxEventAsync(SysNotice notice, CancellationToken ct)
    {
        var userIds = await noticeRepository.ResolveTargetUserIdsAsync(notice, ct);
        if (userIds.Count == 0)
        {
            await RecallInboxEventAsync(notice.Id, ct);
            return;
        }

        await EventPublisher.PublishAsync(new InboxDeliverEvent
        {
            Mode = InboxDeliverModeEnum.ReplaceBySource,
            UserIds = userIds,
            Payload = new InboxMessagePayload
            {
                Title = notice.Title,
                Content = notice.Content,
                SourceType = InboxSourceTypeEnum.Notice,
                SourceId = notice.Id,
                BizModule = "notice",
                BizType = notice.NoticeType.ToString(),
                BizId = notice.Id,
                SenderId = currentAccount.Id,
                SenderName = currentAccount.RealName
            }
        }, ct);
    }

    private static Task RecallInboxEventAsync(long noticeId, CancellationToken ct)
        => EventPublisher.PublishAsync(new InboxRecallEvent
        {
            SourceType = InboxSourceTypeEnum.Notice,
            SourceId = noticeId
        }, ct);

    private static void ApplyInput(SysNotice entity, SaveNoticeInput input)
    {
        entity.Title = input.Title.Trim();
        entity.Content = input.Content.Trim();
        entity.NoticeType = input.NoticeType;
        entity.TargetType = input.TargetType;
        entity.TargetIds = input.TargetType == NoticeTargetTypeEnum.All ? null : input.TargetIds?.Trim();
        entity.OrderNo = input.OrderNo;
        entity.Status = input.Status;
        entity.Remark = input.Remark;
    }
}
