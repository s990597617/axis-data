using Axis.Application.System.User;
using Axis.Application.System.User.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.User;

/// <summary>
/// 创建用户
/// </summary>
public sealed class CreateUserEndpoint(IUserAppService userService)
    : AxisEndpoint<CreateUserInput>
{
    public override void Configure()
    {
        Post("/sys/user");
        Summary(s => s.Summary = "创建用户");
        Options(x => x.WithTags("用户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.User.Create)
            .Transactional());
    }

    public override async Task HandleAsync(CreateUserInput req, CancellationToken ct)
    {
        await userService.CreateAsync(req, ct);
    }
}
