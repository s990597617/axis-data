namespace Axis.Infrastructure.Notify;

/// <summary>
/// 向在线客户端推送强制下线指令。
/// </summary>
public interface ISessionKickPusher
{
    /// <summary>
    /// 通知指定用户强制下线；用户离线时忽略。
    /// </summary>
    /// <param name="userId">目标用户 Id。</param>
    /// <param name="payload">下线原因与会话信息。</param>
    /// <param name="ct">取消令牌。</param>
    Task PushForceLogoutAsync(long userId, ForceLogoutPayload payload, CancellationToken ct = default);
}
