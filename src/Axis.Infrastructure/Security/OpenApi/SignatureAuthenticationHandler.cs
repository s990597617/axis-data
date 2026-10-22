using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Axis.Infrastructure.Security.OpenApi;

/// <summary>
/// OpenAPI HMAC-SHA256 签名认证 Handler；业务验签委托给 <see cref="IOpenApiSignatureAuthService"/>。
/// </summary>
public sealed class SignatureAuthenticationHandler(
    IOptionsMonitor<SignatureAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<SignatureAuthenticationOptions>(options, logger, encoder)
{
    /// <inheritdoc />
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var opt = Options;
        var accessKey = Request.Headers[opt.AccessKeyHeader].ToString().Trim();
        var timestamp = Request.Headers[opt.TimestampHeader].ToString().Trim();
        var nonce = Request.Headers[opt.NonceHeader].ToString().Trim();
        var sign = Request.Headers[opt.SignHeader].ToString().Trim();

        // 未带签名头：交由其它 Scheme（如 JWT）处理。
        if (string.IsNullOrWhiteSpace(accessKey)
            && string.IsNullOrWhiteSpace(timestamp)
            && string.IsNullOrWhiteSpace(nonce)
            && string.IsNullOrWhiteSpace(sign))
        {
            return AuthenticateResult.NoResult();
        }

        if (string.IsNullOrWhiteSpace(accessKey)
            || string.IsNullOrWhiteSpace(timestamp)
            || string.IsNullOrWhiteSpace(nonce)
            || string.IsNullOrWhiteSpace(sign))
        {
            return AuthenticateResult.Fail("缺少 OpenAPI 签名请求头");
        }

        var auth = Context.RequestServices.GetRequiredService<IOpenApiSignatureAuthService>();
        var result = await auth.AuthenticateAsync(
            accessKey,
            timestamp,
            nonce,
            sign,
            Request.Method,
            Request.Path.Value ?? "/",
            opt,
            Context.RequestAborted);

        if (result.Principal is null)
            return AuthenticateResult.Fail(result.FailureMessage ?? "签名认证失败");

        var ticket = new AuthenticationTicket(
            result.Principal,
            SignatureAuthenticationDefaults.AuthenticationScheme);
        return AuthenticateResult.Success(ticket);
    }
}
