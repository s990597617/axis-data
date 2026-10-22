using Axis.Application.System.User;
using Axis.Application.System.User.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.User;

/// <summary>
/// 更新用户
/// </summary>
public sealed class UpdateUserEndpoint(IUserAppService userService)
    : AxisEndpoint<UpdateUserInput>
{
    public override void Configure()
    {
        Put("/sys/user/{Id}");
        Summary(s => s.Summary = "更新用户信息");
        Options(x => x.WithTags("用户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.User.Edit)
            .Transactional());
    }

    public override async Task HandleAsync(UpdateUserInput req, CancellationToken ct)
    {
        await userService.UpdateAsync(req, ct);
    }
}
