using Axis.Application.Author;
using Axis.Application.Author.Dto;
using Axis.Infrastructure.Attributes;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Author;

/// <summary>
/// 获取当前登录用户完整信息（含机构、按钮权限、菜单路由树）。
/// </summary>
/// <remarks>
/// 调用 <see cref="IAuthorProfileAppService"/>，与会话签发服务分离。
/// </remarks>
public sealed class GetUserInfoEndpoint(IAuthorProfileAppService profile)
    : AxisEndpointWithoutRequest<LoginUserInfo>
{
    public override void Configure()
    {
        Get("/sys/author/current-user-info");
        Summary(s =>
        {
            s.Summary = "获取当前登录用户信息";
            s.Description = "返回当前登录用户 + 机构 + 按钮权限 + 菜单树";
        });
        Options(x => x.WithTags("登录授权")
            .WithGroupName(nameof(ApiGroupEnum.AdminAuthorModule))
            .WithMetadata(new VisitLogAttribute()));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await profile.GetUserInfoAsync(ct);
}
