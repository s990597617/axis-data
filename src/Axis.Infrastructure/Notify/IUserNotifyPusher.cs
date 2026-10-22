namespace Axis.Infrastructure.Notify;

/// <summary>
/// 向在线用户推送收件箱未读变更的端口。
/// </summary>
/// <remarks>
/// Host 层以 SignalR 实现；未启用实时通道时由 <see cref="NullUserNotifyPusher"/> 空实现兜底，
/// 保证 Notice / Message / Inbox 用例不依赖具体传输方式。
/// </remarks>
public interface IUserNotifyPusher
{
    /// <summary>
    /// 向指定用户推送未读统计。
    /// </summary>
    /// <param name="userId">目标用户 Id（与 JWT Claim <c>id</c> / SignalR UserId 对齐）。</param>
    /// <param name="payload">未读统计载荷；离线用户可安全忽略。</param>
    /// <param name="ct">取消令牌。</param>
    Task PushUnreadAsync(long userId, UnreadNotifyPayload payload, CancellationToken ct = default);

    /// <summary>
    /// 向多个用户分别推送未读统计；载荷由工厂按用户计算，避免全员共用同一快照。
    /// </summary>
    /// <param name="userIds">目标用户 Id 集合（实现侧应去重）。</param>
    /// <param name="payloadFactory">按用户 Id 异步构建推送载荷。</param>
    /// <param name="ct">取消令牌。</param>
    Task PushUnreadAsync(
        IEnumerable<long> userIds,
        Func<long, Task<UnreadNotifyPayload>> payloadFactory,
        CancellationToken ct = default);
}
