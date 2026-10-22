namespace Axis.Infrastructure.Notify;

/// <summary>
/// 强制下线推送载荷（SignalR <c>ForceLogout</c>）。
/// </summary>
public sealed class ForceLogoutPayload
{
    /// <summary>
    /// 下线原因文案，前端提示用。
    /// </summary>
    public string Reason { get; set; } = "您已被强制下线，请重新登录";

    /// <summary>
    /// 被踢的会话 Id；按用户踢人时可为 null。
    /// </summary>
    public string? SessionId { get; set; }
}
