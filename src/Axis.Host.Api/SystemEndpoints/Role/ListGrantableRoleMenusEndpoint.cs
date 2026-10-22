using Axis.Application.System.Menu.Dto;
using Axis.Application.System.Role;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Role;

/// <summary>
/// 查询当前租户可授予角色的菜单平铺列表。
/// </summary>
public sealed class ListGrantableRoleMenusEndpoint(IRoleAppService service)
    : AxisEndpointWithoutRequest<IReadOnlyList<MenuDto>>
{
    public override void Configure()
    {
        Get("/sys/role/menus/grantable");
        Summary(summary => summary.Summary = "可授权角色菜单");
        Options(options => options.WithTags("角色管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Role.Grant));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.ListGrantableMenusAsync(ct);
}
