using Axis.Application.System.User;
using Axis.Application.System.User.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.User;

/// <summary>
/// 修改用户状态（启用 / 禁用）。
/// </summary>
public sealed class ChangeStatusEndpoint(IUserAppService userService)
    : AxisEndpoint<ChangeUserStatusInput>
{
    public override void Configure()
    {
        Post("/sys/user/{Id}/status");
        Summary(s => s.Summary = "修改用户状态");
        Options(x => x.WithTags("用户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.User.Edit));
    }

    public override async Task HandleAsync(ChangeUserStatusInput req, CancellationToken ct)
        => await userService.ChangeStatusAsync(req.Id, req.Status, ct);
}
