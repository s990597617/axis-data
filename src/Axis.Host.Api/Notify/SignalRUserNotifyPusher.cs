using Axis.Infrastructure.Notify;
using Microsoft.AspNetCore.SignalR;

namespace Axis.Host.Api.Notify;

/// <summary>
/// 基于 SignalR 的未读推送实现。
/// </summary>
public sealed class SignalRUserNotifyPusher(IHubContext<NotifyHub> hubContext) : IUserNotifyPusher
{
    /// <inheritdoc />
    public Task PushUnreadAsync(long userId, UnreadNotifyPayload payload, CancellationToken ct = default)
        => hubContext.Clients.User(userId.ToString())
            .SendAsync(NotifyHub.UnreadChangedEvent, payload, ct);

    /// <inheritdoc />
    public async Task PushUnreadAsync(
        IEnumerable<long> userIds,
        Func<long, Task<UnreadNotifyPayload>> payloadFactory,
        CancellationToken ct = default)
    {
        foreach (var userId in userIds.Distinct())
        {
            ct.ThrowIfCancellationRequested();
            var payload = await payloadFactory(userId);
            await PushUnreadAsync(userId, payload, ct);
        }
    }
}
