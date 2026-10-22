using Axis.Application.Author;
using Axis.Application.Author.Dto;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;
using Axis.Infrastructure.Security;

namespace Axis.Host.Api.SystemEndpoints.Author;

/// <summary>
/// Web 端登录：账号 + 密码 + 图形验证码。
/// </summary>
public sealed class LoginEndpoint(
    IAuthorAppService author,
    IRefreshTokenCookieService refreshTokenCookie) : AxisEndpoint<LoginRequest, WebTokenResponse>
{
    public override void Configure()
    {
        AllowAnonymous();
        Post("/sys/author/login");
        Summary(s =>
        {
            s.Summary = "用户登录接口";
            s.Description = "用户登录接口 - 账号 + 密码 + 图形验证码";
        });
        Options(x => x.WithTags("登录授权")
            .WithGroupName(nameof(ApiGroupEnum.AdminAuthorModule)));
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var tokens = await author.LoginAsync(req, ct);
        refreshTokenCookie.Write(HttpContext, tokens.RefreshToken, tokens.RefreshTokenExpires, tokens.RememberMe);
        DisableCaching();
        Response = WebTokenResponse.From(tokens);
    }

    private void DisableCaching()
    {
        HttpContext.Response.Headers.CacheControl = "no-store";
        HttpContext.Response.Headers.Pragma = "no-cache";
    }
}
