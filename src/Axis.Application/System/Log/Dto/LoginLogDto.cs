namespace Axis.Application.System.Log.Dto;

/// <summary>
/// 登录日志列表项。
/// </summary>
public sealed class LoginLogDto
{
    /// <summary>
    /// 主键。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 用户 Id。
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 登录账号。
    /// </summary>
    public string Account { get; set; } = null!;

    /// <summary>
    /// 客户端 IP；可为空。
    /// </summary>
    public string RemoteIp { get; set; }

    /// <summary>
    /// User-Agent；可为空。
    /// </summary>
    public string UserAgent { get; set; }

    /// <summary>
    /// 是否登录成功。
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 失败原因；可为空。
    /// </summary>
    public string FailReason { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedTime { get; set; }
}
