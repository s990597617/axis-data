using Microsoft.AspNetCore.Authentication;

namespace Axis.Infrastructure.Security.OpenApi;

/// <summary>
/// OpenAPI 签名认证选项（请求头名与时间窗）。
/// </summary>
public sealed class SignatureAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// AccessKey 请求头名。
    /// </summary>
    public string AccessKeyHeader { get; set; } = "accessKey";

    /// <summary>
    /// 时间戳请求头名（Unix 秒）。
    /// </summary>
    public string TimestampHeader { get; set; } = "timestamp";

    /// <summary>
    /// Nonce 请求头名。
    /// </summary>
    public string NonceHeader { get; set; } = "nonce";

    /// <summary>
    /// 签名请求头名（Base64）。
    /// </summary>
    public string SignHeader { get; set; } = "sign";

    /// <summary>
    /// 时间戳允许偏差（秒）；同时作为 Nonce TTL。
    /// </summary>
    public int MaxSkewSeconds { get; set; } = 300;

    /// <summary>
    /// Nonce 最小长度。
    /// </summary>
    public int MinNonceLength { get; set; } = 6;
}
