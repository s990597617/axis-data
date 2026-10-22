using Axis.Application.System.User;
using Axis.Application.System.User.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.User;

/// <summary>
/// 重置用户密码。
/// </summary>
public sealed class ResetPasswordEndpoint(IUserAppService userService)
    : AxisEndpoint<ResetPasswordInput>
{
    public override void Configure()
    {
        Post("/sys/user/{Id}/reset-password");
        Summary(s => s.Summary = "重置用户密码");
        Options(x => x.WithTags("用户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.User.ResetPassword));
    }

    public override async Task HandleAsync(ResetPasswordInput req, CancellationToken ct)
        => await userService.ResetPasswordAsync(req.Id, req.NewPassword, ct);
}
