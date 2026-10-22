using Axis.Application.System.Menu;
using Axis.Application.System.Menu.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Attributes;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Menu;

/// <summary>
/// 查询菜单、目录和按钮列表。
/// </summary>
/// <remarks>
/// 需要菜单查询或角色授权权限之一（角色授权页需拉取菜单树）。
/// </remarks>
public sealed class ListMenusEndpoint(IMenuAppService service)
    : AxisEndpoint<MenuListQuery, IReadOnlyList<MenuDto>>
{
    public override void Configure()
    {
        Get("/sys/menu/list");
        Summary(summary => summary.Summary = "菜单树列表");
        Options(options => options.WithTags("菜单管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(
                PermissionPolicy.AnyOf,
                SystemPermissionCodes.Menu.Query,
                SystemPermissionCodes.Role.Grant));
    }

    public override async Task HandleAsync(MenuListQuery request, CancellationToken ct)
        => Response = await service.ListAsync(request, ct);
}
