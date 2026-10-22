using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Axis.Host.Api.Notify;

/// <summary>
/// 用户通知 Hub：客户端连接后按用户 Id 分组，接收未读数变更。
/// </summary>
[Authorize]
public sealed class NotifyHub : Hub
{
    /// <summary>
    /// 服务端推送给客户端的未读变更事件名。
    /// </summary>
    public const string UnreadChangedEvent = "UnreadChanged";

    /// <summary>
    /// 服务端推送给客户端的强制下线事件名。
    /// </summary>
    public const string ForceLogoutEvent = "ForceLogout";
}
