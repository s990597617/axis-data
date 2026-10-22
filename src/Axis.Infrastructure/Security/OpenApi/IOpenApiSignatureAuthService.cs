using System.Security.Claims;
using Axis.Infrastructure.Security.OpenApi;

namespace Axis.Infrastructure.Security.OpenApi;

/// <summary>
/// OpenAPI 签名验签与主体构造（实现放在 Application，避免 Infrastructure 反向依赖 Domain）。
/// </summary>
public interface IOpenApiSignatureAuthService
{
    /// <summary>
    /// 校验签名头并构造 ClaimsPrincipal；失败返回 null 与错误原因。
    /// </summary>
    /// <param name="accessKey">身份标识。</param>
    /// <param name="timestamp">Unix 秒时间戳原文。</param>
    /// <param name="nonce">一次性随机串。</param>
    /// <param name="sign">Base64 签名。</param>
    /// <param name="method">HTTP 方法。</param>
    /// <param name="path">请求路径。</param>
    /// <param name="options">签名选项。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>成功主体；失败时 Principal 为空。</returns>
    Task<OpenApiSignatureAuthResult> AuthenticateAsync(
        string accessKey,
        string timestamp,
        string nonce,
        string sign,
        string method,
        string path,
        SignatureAuthenticationOptions options,
        CancellationToken ct = default);
}

/// <summary>
/// OpenAPI 签名认证结果。
/// </summary>
/// <param name="Principal">成功时的主体。</param>
/// <param name="FailureMessage">失败原因。</param>
public sealed record OpenApiSignatureAuthResult(ClaimsPrincipal? Principal, string? FailureMessage)
{
    /// <summary>成功结果。</summary>
    public static OpenApiSignatureAuthResult Ok(ClaimsPrincipal principal)
        => new(principal, null);

    /// <summary>失败结果。</summary>
    public static OpenApiSignatureAuthResult Fail(string message)
        => new(null, message);
}
