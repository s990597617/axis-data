using Axis.Application.System.Inbox.Dto;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Notify;

namespace Axis.Application.System.Inbox;

/// <summary>
/// 用户收件箱应用服务：查询、已读与未读统计（读模型）。
/// </summary>
/// <remarks>
/// 跨模块写入请发布 <c>InboxDeliverEvent</c> / <c>InboxRecallEvent</c>，不要在本接口扩展写方法。
/// </remarks>
public interface IInboxAppService
{
    /// <summary>
    /// 获取当前登录用户未读统计。
    /// </summary>
    Task<UnreadCountDto> GetUnreadCountAsync(CancellationToken ct = default);

    /// <summary>
    /// 按用户 Id 计算未读推送载荷。
    /// </summary>
    /// <param name="userId">目标用户 Id。</param>
    /// <param name="ct">取消令牌。</param>
    Task<UnreadNotifyPayload> GetUnreadPayloadAsync(long userId, CancellationToken ct = default);

    /// <summary>
    /// 分页查询当前用户收件箱。
    /// </summary>
    Task<SqlSugarPagedList<InboxItemDto>> PageAsync(InboxPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 获取单条收件箱详情；无权或不存在时返回 null。
    /// </summary>
    /// <param name="id">收件箱主键。</param>
    /// <param name="ct">取消令牌。</param>
    Task<InboxItemDto?> GetAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 标记单条已读并推送最新未读数。
    /// </summary>
    /// <param name="id">收件箱主键。</param>
    /// <param name="ct">取消令牌。</param>
    Task MarkReadAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 全部标为已读并推送最新未读数。
    /// </summary>
    Task MarkAllReadAsync(CancellationToken ct = default);

    /// <summary>
    /// 按来源将当前用户对应收件箱标为已读。
    /// </summary>
    Task MarkReadBySourceAsync(InboxSourceTypeEnum sourceType, long sourceId, CancellationToken ct = default);
}
