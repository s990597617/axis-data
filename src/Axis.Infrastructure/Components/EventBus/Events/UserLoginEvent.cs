namespace Axis.Infrastructure.Components.EventBus.Events;

/// <summary>
/// 用户登录成功事件
/// </summary>
public class UserLoginEvent : AxisEvent
{
    /// <summary>
    /// 用户 ID
    /// </summary>
    public long UserId { get; init; }

    /// <summary>
    /// 用户账号
    /// </summary>
    public string Account { get; init; }

    /// <summary>
    /// 登录 IP
    /// </summary>
    public string RemoteIp { get; init; }

    /// <summary>
    /// User-Agent
    /// </summary>
    public string UserAgent { get; init; }
}
