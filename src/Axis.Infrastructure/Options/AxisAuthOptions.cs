namespace Axis.Infrastructure.Options;

/// <summary>
/// Web 认证与跨域配置。
/// </summary>
public sealed class AxisAuthOptions
{
    /// <summary>
    /// 配置节名称。
    /// </summary>
    public const string SectionName = "Auth";

    /// <summary>
    /// 允许携带凭据访问接口的前端来源，必须是精确 Origin。
    /// </summary>
    public string[] AllowedOrigins { get; set; } = [];

    /// <summary>
    /// Refresh Token Cookie 配置。
    /// </summary>
    public AxisRefreshTokenCookieOptions RefreshTokenCookie { get; set; } = new();

    /// <summary>
    /// 已登录写请求的时间戳 + Nonce + HMAC 防重放配置。
    /// </summary>
    public AxisAntiReplayOptions AntiReplay { get; set; } = new();
}

/// <summary>
/// Refresh Token Cookie 配置。
/// </summary>
public sealed class AxisRefreshTokenCookieOptions
{
    /// <summary>
    /// Cookie 名称。
    /// </summary>
    public string Name { get; set; } = "axis-refresh-token";

    /// <summary>
    /// Cookie Path，需覆盖刷新与登出接口路径。
    /// </summary>
    public string Path { get; set; } = "/api/sys/author";

    /// <summary>
    /// 是否仅 HTTPS 传输。
    /// </summary>
    public bool Secure { get; set; }

    /// <summary>
    /// SameSite 策略字符串（Lax / Strict / None）。
    /// </summary>
    public string SameSite { get; set; } = "Lax";
}

/// <summary>
/// 请求级防重放：时间戳窗口 + 一次性 Nonce + HMAC-SHA256（密钥为 AccessToken）。
/// </summary>
/// <remarks>
/// 仅对已认证的 POST/PUT/PATCH/DELETE 生效；匿名与读请求跳过。
/// 管理端无独立 accessSecret，暂用当前 Bearer Token 作 HMAC 密钥；
/// 后续 OpenAPI 的 AccessKey/Secret 签名走独立认证方案。
/// </remarks>
public sealed class AxisAntiReplayOptions
{
    /// <summary>
    /// 是否启用；关闭后预处理器直接放行。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 是否校验 HMAC 签名；默认开启。
    /// </summary>
    public bool RequireSignature { get; set; } = true;

    /// <summary>
    /// 客户端时间戳相对服务器允许的最大偏差（秒）；同时作为 Nonce 缓存 TTL。
    /// </summary>
    public int MaxSkewSeconds { get; set; } = 300;

    /// <summary>
    /// 时间戳请求头名（Unix 毫秒）。
    /// </summary>
    public string TimestampHeader { get; set; } = "X-Axis-Timestamp";

    /// <summary>
    /// 一次性 Nonce 请求头名。
    /// </summary>
    public string NonceHeader { get; set; } = "X-Axis-Nonce";

    /// <summary>
    /// HMAC 签名请求头名（Base64）。
    /// </summary>
    public string SignHeader { get; set; } = "X-Axis-Sign";

    /// <summary>
    /// Nonce 最小长度（字符）；过短视为无效。
    /// </summary>
    public int MinNonceLength { get; set; } = 16;
}
