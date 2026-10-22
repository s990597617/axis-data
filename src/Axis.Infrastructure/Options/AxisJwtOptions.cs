namespace Axis.Infrastructure.Options;

/// <summary>
/// JWT 相关配置。
/// </summary>
public class AxisJwtOptions
{
    /// <summary>
    /// Token 签名密钥（对称密钥或相关配置材料）。
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// 发行人。
    /// </summary>
    public string Issuer { get; set; }

    /// <summary>
    /// 订阅人（受众）。
    /// </summary>
    public string Audience { get; set; }

    /// <summary>
    /// Access Token 过期时间（分钟等，按配置约定）。
    /// </summary>
    public int TokenExpire { get; set; }

    /// <summary>
    /// Refresh Token 过期时间（分钟等，按配置约定）。
    /// </summary>
    public int RefreshTokenExpire { get; set; }
}
