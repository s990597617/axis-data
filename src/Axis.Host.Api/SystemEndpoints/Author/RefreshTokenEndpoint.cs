using Axis.Application.Author;
using Axis.Application.Author.Dto;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;
using Axis.Infrastructure.Host.Responses;
using Axis.Infrastructure.Security;

namespace Axis.Host.Api.SystemEndpoints.Author;

/// <summary>
/// 使用刷新令牌换取新的 AccessToken，并轮换刷新令牌（一次性）。
/// </summary>
public sealed class RefreshTokenEndpoint(
    IAuthorAppService author,
    IRefreshTokenCookieService refreshTokenCookie) : AxisEndpointWithoutRequest<WebTokenResponse>
{
    public override void Configure()
    {
        AllowAnonymous();
        Post("/sys/author/refresh-token");
        Summary(s =>
        {
            s.Summary = "用户刷新 Token 接口";
            s.Description = "使用刷新令牌换取新 AccessToken，旧刷新令牌失效";
        });
        Options(x => x.WithTags("登录授权")
            .WithGroupName(nameof(ApiGroupEnum.AdminAuthorModule)));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var refreshToken = refreshTokenCookie.Read(HttpContext);
        if (string.IsNullOrWhiteSpace(refreshToken))
            ThrowBusiness(ResultCodes.TokenExpired, "登录已过期，请重新登录！");

        var tokens = await author.RefreshTokenAsync(refreshToken!, ct);
        refreshTokenCookie.Write(HttpContext, tokens.RefreshToken, tokens.RefreshTokenExpires, tokens.RememberMe);
        HttpContext.Response.Headers.CacheControl = "no-store";
        HttpContext.Response.Headers.Pragma = "no-cache";
        Response = WebTokenResponse.From(tokens);
    }
}
