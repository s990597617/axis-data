namespace Axis.Infrastructure.Security.OpenApi;

/// <summary>
/// OpenAPI HMAC 签名认证方案名称。
/// </summary>
public static class SignatureAuthenticationDefaults
{
    /// <summary>
    /// 认证方案名，用于 <c>AuthSchemes</c> / <c>[Authorize(AuthenticationSchemes=...)]</c>。
    /// </summary>
    public const string AuthenticationScheme = "Signature";
}
