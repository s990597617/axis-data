using Axis.Domain.Data.System.Repository;
using Axis.Domain.Enums;
using Axis.Domain.System;

namespace Axis.Infrastructure.Data.System.Repository;

/// <summary>
/// Outbox 仓储实现。
/// </summary>
public sealed class SysOutboxRepository(ISqlSugarClient db, IServiceProvider sp)
    : BaseRepository<SysOutbox>(db, sp), ISysOutboxRepository, IAutoInject
{
    /// <summary>
    /// 插入待投递行。
    /// </summary>
    public Task AddPendingAsync(SysOutbox row, CancellationToken ct = default)
        => Context.Insertable(row).ExecuteCommandAsync(ct);

    /// <summary>
    /// 按事件 Id 查询。
    /// </summary>
    public Task<SysOutbox?> FindByEventIdAsync(long eventId, CancellationToken ct = default)
        => Context.Queryable<SysOutbox>().Where(item => item.EventId == eventId).FirstAsync(ct);

    /// <summary>
    /// 拉取到期行。
    /// </summary>
    public async Task<IReadOnlyList<SysOutbox>> ListDueAsync(
        int batchSize,
        DateTime now,
        CancellationToken ct = default)
    {
        var rows = await Context.Queryable<SysOutbox>()
            .Where(item => item.Status == OutboxStatusEnum.Pending || item.Status == OutboxStatusEnum.Failed)
            .Where(item => item.NextRetryTime == null || item.NextRetryTime <= now)
            .OrderBy(item => item.CreatedTime)
            .Take(batchSize)
            .ToListAsync(ct);
        return rows;
    }

    /// <summary>
    /// 标记投递成功。
    /// </summary>
    public Task MarkPublishedAsync(long id, DateTime publishedTime, CancellationToken ct = default)
        => Context.Updateable<SysOutbox>()
            .SetColumns(item => item.Status == OutboxStatusEnum.Published)
            .SetColumns(item => item.PublishedTime == publishedTime)
            .SetColumns(item => item.LastError == null)
            .Where(item => item.Id == id)
            .ExecuteCommandAsync(ct);

    /// <summary>
    /// 标记失败或死信。
    /// </summary>
    public Task MarkFailedAsync(
        long id,
        OutboxStatusEnum status,
        int retryCount,
        DateTime? nextRetryTime,
        string? error,
        CancellationToken ct = default)
        => Context.Updateable<SysOutbox>()
            .SetColumns(item => item.Status == status)
            .SetColumns(item => item.RetryCount == retryCount)
            .SetColumns(item => item.NextRetryTime == nextRetryTime)
            .SetColumns(item => item.LastError == error)
            .Where(item => item.Id == id)
            .ExecuteCommandAsync(ct);
}
