namespace Axis.Application.System.Online.Dto;

/// <summary>
/// 按用户强制下线的路径参数。
/// </summary>
public sealed class KickOnlineUserRequest
{
    /// <summary>用户主键。</summary>
    public long UserId { get; set; }
}
