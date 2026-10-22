namespace Axis.Infrastructure.Author.Internal;

/// <summary>
/// 平台令牌值
/// </summary>
public class ClaimConst
{
    /// <summary>
    /// 租户编号
    /// </summary>
    public const string TenantId = "td";

    /// <summary>
    /// 账户编号
    /// </summary>
    public const string AccountId = "id";

    /// <summary>
    /// 账户名称
    /// </summary>
    public const string AccountName = "an";

    /// <summary>
    /// 账户真实姓名
    /// </summary>
    public const string RealName = "rn";

    /// <summary>
    /// 账户类型
    /// </summary>
    public const string AccountType = "at";

    /// <summary>
    /// 组织机构 id
    /// </summary>
    public const string OrgId = "oi";

    /// <summary>
    /// 组织机构名称
    /// </summary>
    public const string OrgName = "on";

    /// <summary>
    /// AccessToken 唯一标识（jti），用于强制下线吊销。
    /// </summary>
    public const string JwtId = "jti";
}