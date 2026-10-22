using Axis.Domain.Data.System.Repository;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Dependency.Annotations;

namespace Axis.Infrastructure.Data.System.Repository;

/// <summary>
/// 用户收件箱仓储实现。
/// </summary>
public sealed class SysInboxRepository(ISqlSugarClient db, IServiceProvider sp)
    : BaseRepository<SysInbox>(db, sp), ISysInboxRepository, IAutoInject
{
    /// <inheritdoc />
    public Task<SqlSugarPagedList<SysInbox>> PageByUserAsync(
        long userId,
        int page,
        int pageSize,
        string keyword,
        InboxSourceTypeEnum? sourceType,
        bool? isRead,
        CancellationToken ct = default)
    {
        var query = Context.Queryable<SysInbox>()
            .Where(item => item.UserId == userId)
            .WhereIF(sourceType.HasValue, item => item.SourceType == sourceType!.Value)
            .WhereIF(isRead.HasValue, item => item.IsRead == isRead!.Value)
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), item =>
                item.Title.Contains(keyword!)
                || (item.Summary != null && item.Summary.Contains(keyword!)))
            .OrderBy(item => item.Priority, OrderByType.Desc)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new SysInbox
            {
                Id = item.Id,
                UserId = item.UserId,
                Title = item.Title,
                Summary = item.Summary,
                SourceType = item.SourceType,
                SourceId = item.SourceId,
                BizModule = item.BizModule,
                BizType = item.BizType,
                BizId = item.BizId,
                LinkPath = item.LinkPath,
                SenderId = item.SenderId,
                SenderName = item.SenderName,
                IsRead = item.IsRead,
                ReadTime = item.ReadTime,
                Priority = item.Priority,
                CreatedTime = item.CreatedTime
            });

        return query.ToPagedListAsync(page, pageSize);
    }

    /// <inheritdoc />
    public Task<int> CountUnreadAsync(long userId, CancellationToken ct = default)
        => Context.Queryable<SysInbox>()
            .Where(item => item.UserId == userId && !item.IsRead)
            .CountAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<InboxSourceTypeEnum, int>> CountUnreadBySourceAsync(
        long userId,
        CancellationToken ct = default)
    {
        var types = await Context.Queryable<SysInbox>()
            .Where(item => item.UserId == userId && !item.IsRead)
            .Select(item => item.SourceType)
            .ToListAsync(ct);

        return types
            .GroupBy(item => item)
            .ToDictionary(group => group.Key, group => group.Count());
    }

    /// <inheritdoc />
    public Task<SysInbox> GetOwnedAsync(long inboxId, long userId, CancellationToken ct = default)
        => Context.Queryable<SysInbox>()
            .Where(item => item.Id == inboxId && item.UserId == userId)
            .FirstAsync(ct);

    /// <inheritdoc />
    public async Task InsertDeliveriesAsync(IReadOnlyList<SysInbox> rows, CancellationToken ct = default)
    {
        if (rows.Count == 0) return;
        await Context.Insertable(rows.ToList()).ExecuteCommandAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<long>> ListUserIdsBySourceAsync(
        InboxSourceTypeEnum sourceType,
        long sourceId,
        CancellationToken ct = default)
    {
        var userIds = await Context.Queryable<SysInbox>()
            .Where(item => item.SourceType == sourceType && item.SourceId == sourceId)
            .Select(item => item.UserId)
            .Distinct()
            .ToListAsync(ct);
        return userIds;
    }

    /// <inheritdoc />
    public Task DeleteBySourceAsync(InboxSourceTypeEnum sourceType, long sourceId, CancellationToken ct = default)
        => Context.Deleteable<SysInbox>()
            .Where(item => item.SourceType == sourceType && item.SourceId == sourceId)
            .ExecuteCommandAsync(ct);

    /// <inheritdoc />
    public async Task<bool> MarkReadAsync(long inboxId, long userId, CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var affected = await Context.Updateable<SysInbox>()
            .SetColumns(item => new SysInbox { IsRead = true, ReadTime = now })
            .Where(item => item.Id == inboxId && item.UserId == userId && !item.IsRead)
            .ExecuteCommandAsync(ct);
        return affected > 0;
    }

    /// <inheritdoc />
    public async Task<bool> MarkReadBySourceAsync(
        long userId,
        InboxSourceTypeEnum sourceType,
        long sourceId,
        CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var affected = await Context.Updateable<SysInbox>()
            .SetColumns(item => new SysInbox { IsRead = true, ReadTime = now })
            .Where(item =>
                item.UserId == userId
                && item.SourceType == sourceType
                && item.SourceId == sourceId
                && !item.IsRead)
            .ExecuteCommandAsync(ct);
        return affected > 0;
    }

    /// <inheritdoc />
    public Task MarkAllReadAsync(long userId, CancellationToken ct = default)
    {
        var now = DateTime.Now;
        return Context.Updateable<SysInbox>()
            .SetColumns(item => new SysInbox { IsRead = true, ReadTime = now })
            .Where(item => item.UserId == userId && !item.IsRead)
            .ExecuteCommandAsync(ct);
    }
}
