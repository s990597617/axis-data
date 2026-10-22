using Axis.Application.System.Profile;
using Axis.Application.System.Profile.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Profile;

/// <summary>
/// 获取当前登录用户的个人中心资料。
/// </summary>
public sealed class GetProfileEndpoint(IProfileAppService profileService)
    : AxisEndpointWithoutRequest<ProfileDto>
{
    public override void Configure()
    {
        Get("/sys/profile");
        Summary(s => s.Summary = "获取个人中心资料");
        Options(x => x.WithTags("个人中心")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Profile.Query));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await profileService.GetAsync(ct);
}
