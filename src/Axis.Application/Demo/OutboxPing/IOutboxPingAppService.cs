using Axis.Application.Demo.OutboxPing.Dto;

namespace Axis.Application.Demo.OutboxPing;

/// <summary>
/// Outbox 联调探测。
/// </summary>
public interface IOutboxPingAppService
{
    /// <summary>
    /// 入队一条探测事件；可选择入队后抛错以验证回滚。
    /// </summary>
    /// <param name="input">探测入参。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>事件 Id 与当时状态。</returns>
    Task<OutboxPingDto> PingAsync(OutboxPingInput input, CancellationToken ct = default);

    /// <summary>
    /// 查询探测事件的 Outbox 状态。
    /// </summary>
    /// <param name="eventId">事件 Id。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>状态；不存在时抛业务异常。</returns>
    Task<OutboxPingDto> GetAsync(long eventId, CancellationToken ct = default);
}
