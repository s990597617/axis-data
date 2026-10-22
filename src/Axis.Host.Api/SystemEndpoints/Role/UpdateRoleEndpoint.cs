using Axis.Application.System.Role;
using Axis.Application.System.Role.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Role;

/// <summary>
/// 更新角色主表字段。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Role.Edit"/> 权限；路由绑定 <c>Id</c>。
/// </remarks>
public sealed class UpdateRoleEndpoint(IRoleAppService service) : AxisEndpoint<SaveRoleInput>
{
    public override void Configure()
    {
        Put("/sys/role/{Id}");
        Summary(summary => summary.Summary = "更新角色");
        Options(options => options.WithTags("角色管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Role.Edit));
    }

    public override async Task HandleAsync(SaveRoleInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
