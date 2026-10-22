using Axis.Host.Api.Notify;
using Axis.Infrastructure.Notify;
using Microsoft.AspNetCore.SignalR;

namespace Axis.Host.Api.Notify;

/// <summary>
/// 基于 SignalR 的强制下线推送。
/// </summary>
public sealed class SignalRSessionKickPusher(IHubContext<NotifyHub> hubContext) : ISessionKickPusher
{
    /// <inheritdoc />
    public Task PushForceLogoutAsync(long userId, ForceLogoutPayload payload, CancellationToken ct = default)
        => hubContext.Clients.User(userId.ToString())
            .SendAsync(NotifyHub.ForceLogoutEvent, payload, ct);
}
