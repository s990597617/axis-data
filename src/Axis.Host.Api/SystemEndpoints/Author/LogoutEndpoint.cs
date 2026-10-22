using Axis.Application.Author;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;
using Axis.Infrastructure.Security;

namespace Axis.Host.Api.SystemEndpoints.Author;

/// <summary>
/// 撤销 Refresh Token 并删除 HttpOnly Cookie。
/// </summary>
public sealed class LogoutEndpoint(
    IAuthorAppService author,
    IRefreshTokenCookieService refreshTokenCookie) : AxisEndpointWithoutRequest
{
    public override void Configure()
    {
        AllowAnonymous();
        Post("/sys/author/logout");
        Summary(s =>
        {
            s.Summary = "用户退出登录";
            s.Description = "撤销当前 Refresh Token 并删除 HttpOnly Cookie";
        });
        Options(x => x.WithTags("登录授权")
            .WithGroupName(nameof(ApiGroupEnum.AdminAuthorModule)));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var refreshToken = refreshTokenCookie.Read(HttpContext);
        try
        {
            if (!string.IsNullOrWhiteSpace(refreshToken))
                await author.LogoutAsync(refreshToken, ct);
        }
        finally
        {
            refreshTokenCookie.Delete(HttpContext);
        }

        await Send.NoContentAsync(ct);
    }
}
