using Axis.Application.System.Role;
using Axis.Application.System.Role.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Role;

/// <summary>
/// 查询角色已分配的菜单与按钮 Id。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Role.Grant"/> 权限。
/// </remarks>
public sealed class GetRoleMenusEndpoint(IRoleAppService service)
    : AxisEndpoint<RoleIdRequest, IReadOnlyList<long>>
{
    public override void Configure()
    {
        Get("/sys/role/{Id}/menus");
        Summary(summary => summary.Summary = "查询角色权限");
        Options(options => options.WithTags("角色管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Role.Grant));
    }

    public override async Task HandleAsync(RoleIdRequest request, CancellationToken ct)
        => Response = await service.GetMenuIdsAsync(request.Id, ct);
}
