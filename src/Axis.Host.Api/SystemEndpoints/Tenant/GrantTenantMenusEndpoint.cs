using Axis.Application.System.Tenant;
using Axis.Application.System.Tenant.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Tenant;

/// <summary>
/// 覆盖保存租户菜单启用集。
/// </summary>
public sealed class GrantTenantMenusEndpoint(ITenantAppService service) : AxisEndpoint<GrantTenantMenusInput>
{
    public override void Configure()
    {
        Put("/sys/tenant/{Id}/menus");
        Summary(summary => summary.Summary = "保存租户菜单");
        Options(options => options.WithTags("租户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Tenant.Grant)
            .Transactional());
    }

    public override async Task HandleAsync(GrantTenantMenusInput request, CancellationToken ct)
        => await service.GrantMenusAsync(request.Id, request.MenuIds, ct);
}
