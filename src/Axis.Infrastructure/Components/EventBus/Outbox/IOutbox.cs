namespace Axis.Infrastructure.Components.EventBus.Outbox;

/// <summary>
/// 集成事件入队口。
/// </summary>
/// <remarks>
/// 必须在业务事务内调用；提交后由 Dispatcher 投递。Application 禁止直接写 Outbox 表。
/// </remarks>
public interface IOutbox
{
    /// <summary>
    /// 将集成事件写入 Outbox，与当前工作单元同事务。
    /// </summary>
    /// <param name="event">集成事件。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>事件 Id。</returns>
    Task<long> EnqueueAsync(IIntegrationEvent @event, CancellationToken ct = default);
}
