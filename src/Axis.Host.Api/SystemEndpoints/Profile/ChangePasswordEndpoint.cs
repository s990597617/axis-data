using Axis.Application.System.Profile;
using Axis.Application.System.Profile.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Profile;

/// <summary>
/// 修改当前登录用户密码。
/// </summary>
public sealed class ChangePasswordEndpoint(IProfileAppService profileService)
    : AxisEndpoint<ChangePasswordInput>
{
    public override void Configure()
    {
        Post("/sys/profile/change-password");
        Summary(s => s.Summary = "修改个人登录密码");
        Options(x => x.WithTags("个人中心")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Profile.ChangePassword));
    }

    public override async Task HandleAsync(ChangePasswordInput req, CancellationToken ct)
        => await profileService.ChangePasswordAsync(req, ct);
}
