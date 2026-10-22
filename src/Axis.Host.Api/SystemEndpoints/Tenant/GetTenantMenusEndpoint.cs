using Axis.Application.System.Tenant;
using Axis.Application.System.Tenant.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Tenant;

/// <summary>
/// 查询租户已启用的菜单与按钮 Id。
/// </summary>
public sealed class GetTenantMenusEndpoint(ITenantAppService service)
    : AxisEndpoint<TenantIdRequest, IReadOnlyList<long>>
{
    public override void Configure()
    {
        Get("/sys/tenant/{Id}/menus");
        Summary(summary => summary.Summary = "查询租户菜单");
        Options(options => options.WithTags("租户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Tenant.Grant));
    }

    public override async Task HandleAsync(TenantIdRequest request, CancellationToken ct)
        => Response = await service.GetMenuIdsAsync(request.Id, ct);
}
