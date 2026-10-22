using Axis.Application.System.User;
using Axis.Application.System.User.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.User;

/// <summary>
/// 删除用户。
/// </summary>
public sealed class DeleteUserEndpoint(IUserAppService userService)
    : AxisEndpoint<UserIdRequest>
{
    public override void Configure()
    {
        Delete("/sys/user/{Id}");
        Summary(s => s.Summary = "删除用户");
        Options(x => x.WithTags("用户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.User.Delete)
            .Transactional());
    }

    public override async Task HandleAsync(UserIdRequest req, CancellationToken ct)
        => await userService.DeleteAsync(req.Id, ct);
}
