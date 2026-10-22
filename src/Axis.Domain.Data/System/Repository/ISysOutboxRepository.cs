using Axis.Domain.System;

namespace Axis.Domain.Data.System.Repository;

/// <summary>
/// Outbox 仓储。
/// </summary>
/// <remarks>
/// 仅负责入队与扫描待投递行；序列化与投递在 Infrastructure EventBus。
/// </remarks>
public interface ISysOutboxRepository
{
    /// <summary>
    /// 插入待投递行；调用方须处于业务事务中。
    /// </summary>
    /// <param name="row">Outbox 行。</param>
    /// <param name="ct">取消令牌。</param>
    Task AddPendingAsync(SysOutbox row, CancellationToken ct = default);

    /// <summary>
    /// 按事件 Id 查询。
    /// </summary>
    /// <param name="eventId">事件 Id。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>行；不存在时为 <c>null</c>。</returns>
    Task<SysOutbox?> FindByEventIdAsync(long eventId, CancellationToken ct = default);

    /// <summary>
    /// 拉取到期的待投递 / 失败待重试行。
    /// </summary>
    /// <param name="batchSize">本批上限。</param>
    /// <param name="now">当前时间。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>到期行，按创建时间升序。</returns>
    Task<IReadOnlyList<SysOutbox>> ListDueAsync(int batchSize, DateTime now, CancellationToken ct = default);

    /// <summary>
    /// 标记投递成功。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="publishedTime">投递时间。</param>
    /// <param name="ct">取消令牌。</param>
    Task MarkPublishedAsync(long id, DateTime publishedTime, CancellationToken ct = default);

    /// <summary>
    /// 标记失败或死信。
    /// </summary>
    /// <param name="id">主键。</param>
    /// <param name="status">失败或死信。</param>
    /// <param name="retryCount">新的重试次数。</param>
    /// <param name="nextRetryTime">下次投递；死信为空。</param>
    /// <param name="error">截断后的错误。</param>
    /// <param name="ct">取消令牌。</param>
    Task MarkFailedAsync(
        long id,
        Axis.Domain.Enums.OutboxStatusEnum status,
        int retryCount,
        DateTime? nextRetryTime,
        string? error,
        CancellationToken ct = default);
}
