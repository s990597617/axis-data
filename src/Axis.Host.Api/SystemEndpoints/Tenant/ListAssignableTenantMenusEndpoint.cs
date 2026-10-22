using Axis.Application.System.Menu.Dto;
using Axis.Application.System.Tenant;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Tenant;

/// <summary>
/// 查询可分配给租户的菜单平铺列表（排除平台专属）。
/// </summary>
public sealed class ListAssignableTenantMenusEndpoint(ITenantAppService service)
    : AxisEndpointWithoutRequest<IReadOnlyList<MenuDto>>
{
    public override void Configure()
    {
        Get("/sys/tenant/menus/assignable");
        Summary(summary => summary.Summary = "可分配租户菜单");
        Options(options => options.WithTags("租户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Tenant.Grant));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.ListAssignableMenusAsync(ct);
}
