using Axis.Application.System.User;
using Axis.Application.System.User.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.User;

/// <summary>
/// 获取用户详情。
/// </summary>
public sealed class GetUserEndpoint(IUserAppService userService)
    : AxisEndpoint<UserIdRequest, UserDto>
{
    public override void Configure()
    {
        Get("/sys/user/{Id}");
        Summary(s => s.Summary = "获取用户详情");
        Options(x => x.WithTags("用户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.User.Detail));
    }

    public override async Task HandleAsync(UserIdRequest req, CancellationToken ct) =>
        Response = EnsureFound(value: await userService.GetAsync(req.Id, ct), "用户不存在")!;
}
