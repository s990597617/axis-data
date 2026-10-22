namespace Axis.Infrastructure.Author;

/// <summary>
/// 当前登录用户接口类
/// </summary>
public interface ICurrentAccount
{
    /// <summary>
    /// 租户编号。
    /// </summary>
    /// <remarks>
    /// HTTP 请求读 JWT；Job / Outbox 进入 <c>ITenantScope</c> 后读作用域覆盖。
    /// </remarks>
    long? TenantId { get; }

    /// <summary>
    /// 当前登录用户 id
    /// </summary>
    long Id { get; }

    /// <summary>
    /// 账户名称
    /// </summary>
    string AccountName { get; }

    /// <summary>
    /// 账户真实姓名
    /// </summary>
    string RealName { get; }

    /// <summary>
    /// 账号类型（0：其它，1：普通账号，10：系统管理员，999：超级管理员）
    /// </summary>
    int AccountType { get; }

    /// <summary>
    /// 是否为超级管理员
    /// </summary>
    bool IsSuperAdmin { get; }

    /// <summary>
    /// 当前登录用户组织 id
    /// </summary>
    long OrgId { get; }

    /// <summary>
    /// 当前登录用户组织名称
    /// </summary>
    string OrgName { get; }

    /// <summary>
    /// 当前 AccessToken 的 jti；未登录时为空。
    /// </summary>
    string? JwtId { get; }

    /// <summary>
    /// 当前用户 Ip（包含 IP4 和 IP6）
    /// </summary>
    string Ip { get; }

    /// <summary>
    /// 当前用户 IPv4
    /// </summary>
    string Pv4 { get; }

    /// <summary>
    /// 当前用户 IPv6
    /// </summary>
    string Pv6 { get; }
}