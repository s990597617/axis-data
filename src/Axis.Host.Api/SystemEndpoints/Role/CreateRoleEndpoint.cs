using Axis.Application.System.Role;
using Axis.Application.System.Role.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Role;

/// <summary>
/// 新增角色。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Role.Create"/> 权限。
/// </remarks>
public sealed class CreateRoleEndpoint(IRoleAppService service) : AxisEndpoint<SaveRoleInput>
{
    public override void Configure()
    {
        Post("/sys/role");
        Summary(summary => summary.Summary = "新增角色");
        Options(options => options.WithTags("角色管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Role.Create));
    }

    public override async Task HandleAsync(SaveRoleInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
