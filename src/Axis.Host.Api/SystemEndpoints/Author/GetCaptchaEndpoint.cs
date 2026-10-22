using Axis.Application.Author;
using Axis.Application.Author.Dto;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Author;

/// <summary>
/// 获取图形验证码（用于 Web 登录页）。
/// </summary>
public sealed class GetCaptchaEndpoint(IAuthorAppService author) : AxisEndpointWithoutRequest<CaptchaResponse>
{
    public override void Configure()
    {
        AllowAnonymous();
        Get("/sys/author/captcha");
        Summary(s =>
        {
            s.Summary = "获取验证码接口";
            s.Description = "获取验证码接口 - 登录页调用，返回 base64 图片与对应 Id";
        });
        Options(x => x.WithTags("登录授权")
            .WithGroupName(nameof(ApiGroupEnum.AdminAuthorModule)));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        Response = await author.GetCaptchaAsync(ct);
    }
}
