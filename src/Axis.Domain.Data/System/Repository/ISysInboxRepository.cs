using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;

namespace Axis.Domain.Data.System.Repository;

/// <summary>
/// 用户收件箱仓储。
/// </summary>
public interface ISysInboxRepository : IRepository<SysInbox>
{
    /// <summary>
    /// 分页查询指定用户的收件箱（列表字段，不含正文 Content）。
    /// </summary>
    Task<SqlSugarPagedList<SysInbox>> PageByUserAsync(
        long userId,
        int page,
        int pageSize,
        string? keyword,
        InboxSourceTypeEnum? sourceType,
        bool? isRead,
        CancellationToken ct = default);

    /// <summary>
    /// 统计指定用户未读总数。
    /// </summary>
    Task<int> CountUnreadAsync(long userId, CancellationToken ct = default);

    /// <summary>
    /// 按来源类型统计未读数。
    /// </summary>
    Task<IReadOnlyDictionary<InboxSourceTypeEnum, int>> CountUnreadBySourceAsync(
        long userId,
        CancellationToken ct = default);

    /// <summary>
    /// 获取属于指定用户的收件箱记录。
    /// </summary>
    Task<SysInbox> GetOwnedAsync(long inboxId, long userId, CancellationToken ct = default);

    /// <summary>
    /// 批量投递；同一用户 + 来源若已存在则跳过（幂等）。
    /// </summary>
    Task InsertDeliveriesAsync(IReadOnlyList<SysInbox> rows, CancellationToken ct = default);

    /// <summary>
    /// 列出指定来源下的全部收件人 Id（撤回前用于未读推送）。
    /// </summary>
    Task<IReadOnlyList<long>> ListUserIdsBySourceAsync(
        InboxSourceTypeEnum sourceType,
        long sourceId,
        CancellationToken ct = default);

    /// <summary>
    /// 按来源删除全部收件记录（撤回 / 下架 / 删除业务单据）。
    /// </summary>
    Task DeleteBySourceAsync(InboxSourceTypeEnum sourceType, long sourceId, CancellationToken ct = default);

    /// <summary>
    /// 标记单条已读（仅本人）。
    /// </summary>
    /// <returns>是否实际更新。</returns>
    Task<bool> MarkReadAsync(long inboxId, long userId, CancellationToken ct = default);

    /// <summary>
    /// 按来源标记当前用户已读。
    /// </summary>
    Task<bool> MarkReadBySourceAsync(
        long userId,
        InboxSourceTypeEnum sourceType,
        long sourceId,
        CancellationToken ct = default);

    /// <summary>
    /// 当前用户全部标为已读。
    /// </summary>
    Task MarkAllReadAsync(long userId, CancellationToken ct = default);
}
