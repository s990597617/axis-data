using System.Net.Sockets;
using System.Security.Claims;
using Axis.Infrastructure.Author.Internal;
using Microsoft.AspNetCore.Http;

namespace Axis.Infrastructure.Author;

/// <summary>
/// 当前登录用户信息。
/// </summary>
public class CurrentAccount(IHttpContextAccessor contextAccessor) : ICurrentAccount, IAutoInject
{
    /// <summary>
    /// 当前用户令牌
    /// </summary>
    private ClaimsPrincipal User => contextAccessor?.HttpContext?.User;
    
    /// <summary>
    /// 当前远程 ip
    /// </summary>
    private System.Net.IPAddress RemoteIp => contextAccessor?.HttpContext?.Connection.RemoteIpAddress;

    private string GetClaim(string type)
    {
        var value = User?.FindFirst(type)?.Value;
        return string.IsNullOrEmpty(value) ? null : value;
    }

    private long GetLongClaim(string type) => long.TryParse(GetClaim(type), out var v) ? v : 0L;
    private int GetIntClaim(string type) => int.TryParse(GetClaim(type), out var v) ? v : 0;

    /// <summary>
    /// 租户编号。
    /// </summary>
    /// <remarks>
    /// 优先取 <see cref="TenantScopeContext"/>（Job / Outbox）；否则读 JWT。
    /// </remarks>
    public long? TenantId
    {
        get
        {
            var scoped = TenantScopeContext.Current;
            if (scoped is not null)
                return scoped.Unrestricted ? null : scoped.TenantId;
            return long.TryParse(GetClaim(ClaimConst.TenantId), out var v) ? v : null;
        }
    }

    /// <summary>
    /// 当前登录用户 id
    /// </summary>
    public long Id => GetLongClaim(ClaimConst.AccountId);

    /// <summary>
    /// 账户名称
    /// </summary>
    public string AccountName => GetClaim(ClaimConst.AccountName) ?? string.Empty;

    /// <summary>
    /// 账户真实姓名
    /// </summary>
    public string RealName => GetClaim(ClaimConst.RealName) ?? string.Empty;

    /// <summary>
    /// 账号类型（0：其它，1：普通账号，10：系统管理员，999：超级管理员）
    /// </summary>
    public int AccountType => GetIntClaim(ClaimConst.AccountType);

    /// <summary>
    /// 是否为超级管理员
    /// </summary>
    public bool IsSuperAdmin => AccountType == (int)AccountTypeEnum.SuperAdmin;

    /// <summary>
    /// 当前登录用户组织 id
    /// </summary>
    public long OrgId => GetLongClaim(ClaimConst.OrgId);

    /// <summary>
    /// 当前登录用户组织名称
    /// </summary>
    public string OrgName => GetClaim(ClaimConst.OrgName) ?? string.Empty;

    /// <summary>
    /// 当前 AccessToken 的 jti
    /// </summary>
    public string? JwtId => GetClaim(ClaimConst.JwtId);

    /// <summary>
    /// 当前用户 IP（含 IPv4 与 IPv6）
    /// </summary>
    public string Ip => RemoteIp?.ToString() ?? string.Empty;

    /// <summary>
    /// 当前用户 IPv4（非 IPv4 映射场景返回空串）
    /// </summary>
    public string Pv4
    {
        get
        {
            var ip = RemoteIp;
            if (ip is null) return string.Empty;
            // IPv6 但非 v4 映射地址调用 MapToIPv4 会返回错误地址，提前过滤
            if (ip.AddressFamily == AddressFamily.InterNetwork) return ip.ToString();
            if (ip.IsIPv4MappedToIPv6) return ip.MapToIPv4().ToString();
            return string.Empty;
        }
    }

    /// <summary>
    /// 当前用户 IPv6
    /// </summary>
    public string Pv6
    {
        get
        {
            var ip = RemoteIp;
            if (ip is null) return string.Empty;
            return ip.AddressFamily == AddressFamily.InterNetworkV6
                ? ip.ToString()
                : ip.MapToIPv6().ToString();
        }
    }
}