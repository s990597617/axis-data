using Axis.Application.System.Online.Dto;
using Axis.Infrastructure.Components.Database.Internal;

namespace Axis.Application.System.Online;

/// <summary>
/// 在线用户 / 会话管理契约。
/// </summary>
public interface IOnlineSessionAppService
{
    /// <summary>
    /// 分页查询仍有效的登录会话。
    /// </summary>
    Task<SqlSugarPagedList<OnlineSessionDto>> PageAsync(OnlineSessionPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 强制下线指定会话（吊销 RT + AT + SignalR）。
    /// </summary>
    /// <param name="sessionId">RefreshToken 主键。</param>
    /// <param name="reason">下线原因。</param>
    /// <param name="ct">取消令牌。</param>
    Task ForceLogoutSessionAsync(long sessionId, string? reason = null, CancellationToken ct = default);

    /// <summary>
    /// 强制下线用户全部会话。
    /// </summary>
    /// <param name="userId">用户主键。</param>
    /// <param name="reason">下线原因。</param>
    /// <param name="checkPermission">是否校验「不可踢超管」等规则；系统内部联动可关闭。</param>
    /// <param name="ct">取消令牌。</param>
    Task ForceLogoutUserAsync(
        long userId,
        string? reason = null,
        bool checkPermission = true,
        CancellationToken ct = default);
}
