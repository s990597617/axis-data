namespace Axis.Domain.Data.System.Repository.Dto;

/// <summary>
/// 在线会话列表查询行：有效刷新令牌与用户基础信息的联表投影。
/// </summary>
/// <remarks>
/// 仅供仓储只读分页返回；不映射为实体，不含令牌哈希等敏感字段。
/// 对外 API 出参请使用 Application 层 <c>OnlineSessionDto</c>。
/// </remarks>
public sealed class ActiveSessionDto
{
    /// <summary>
    /// 会话主键，对应 <c>sys_refresh_token.Id</c>。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 会话所属用户主键。
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 登录账号；来自用户表。
    /// </summary>
    public string Account { get; set; } = string.Empty;

    /// <summary>
    /// 真实姓名；可为空。
    /// </summary>
    public string? RealName { get; set; }

    /// <summary>
    /// 账号类型枚举整型值；与 <c>AccountTypeEnum</c> 对齐。
    /// </summary>
    public int AccountType { get; set; }

    /// <summary>
    /// 配对 AccessToken 的 jti；强制下线时写入拒绝名单；可为空。
    /// </summary>
    public string? AccessTokenJti { get; set; }

    /// <summary>
    /// 配对 AccessToken 过期时间；用于拒绝名单 TTL；可为空。
    /// </summary>
    public DateTime? AccessTokenExpiresAt { get; set; }

    /// <summary>
    /// 登录时客户端 IP；可为空。
    /// </summary>
    public string? ClientIp { get; set; }

    /// <summary>
    /// 登录时 User-Agent；可为空；最长 512。
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 会话创建时间（服务器本地时间）。
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 刷新令牌过期时间；列表仅包含尚未过期的记录。
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// 是否记住登录；可为空表示历史数据未记录。
    /// </summary>
    public bool? RememberMe { get; set; }
}
