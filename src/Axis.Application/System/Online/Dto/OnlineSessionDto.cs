namespace Axis.Application.System.Online.Dto;

/// <summary>
/// 在线会话列表项。
/// </summary>
public sealed class OnlineSessionDto
{
    /// <summary>会话 Id。</summary>
    public long Id { get; set; }

    /// <summary>用户 Id。</summary>
    public long UserId { get; set; }

    /// <summary>登录账号。</summary>
    public string Account { get; set; } = string.Empty;

    /// <summary>真实姓名。</summary>
    public string? RealName { get; set; }

    /// <summary>账号类型。</summary>
    public int AccountType { get; set; }

    /// <summary>客户端 IP。</summary>
    public string? ClientIp { get; set; }

    /// <summary>User-Agent。</summary>
    public string? UserAgent { get; set; }

    /// <summary>登录时间。</summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>RefreshToken 过期时间。</summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>是否记住登录。</summary>
    public bool RememberMe { get; set; }

    /// <summary>是否当前操作者自己的会话。</summary>
    public bool IsCurrent { get; set; }
}
