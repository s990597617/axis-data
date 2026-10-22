namespace Axis.Application.System.Online.Dto;

/// <summary>
/// 按会话强制下线的路径参数。
/// </summary>
public sealed class KickOnlineSessionRequest
{
    /// <summary>会话 Id（RefreshToken 主键）。</summary>
    public long SessionId { get; set; }
}
