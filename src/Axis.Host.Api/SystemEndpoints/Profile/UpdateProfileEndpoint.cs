using Axis.Application.System.Profile;
using Axis.Application.System.Profile.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Profile;

/// <summary>
/// 更新当前登录用户基础资料。
/// </summary>
public sealed class UpdateProfileEndpoint(IProfileAppService profileService)
    : AxisEndpoint<UpdateProfileInput>
{
    public override void Configure()
    {
        Put("/sys/profile");
        Summary(s => s.Summary = "更新个人基础资料");
        Options(x => x.WithTags("个人中心")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Profile.Update));
    }

    public override async Task HandleAsync(UpdateProfileInput req, CancellationToken ct)
        => await profileService.UpdateAsync(req, ct);
}
