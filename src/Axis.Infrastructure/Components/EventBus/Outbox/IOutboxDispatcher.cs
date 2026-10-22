namespace Axis.Infrastructure.Components.EventBus.Outbox;

/// <summary>
/// 扫描到期 Outbox 行并经配置的传输投递。
/// </summary>
public interface IOutboxDispatcher
{
    /// <summary>
    /// 处理一批到期消息。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>本批尝试条数。</returns>
    Task<int> DispatchOnceAsync(CancellationToken ct = default);
}
