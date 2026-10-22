using Axis.Application.System.Tenant;
using Axis.Application.System.Tenant.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Tenant;

/// <summary>
/// 分页查询租户。
/// </summary>
public sealed class PageTenantsEndpoint(ITenantAppService service)
    : AxisEndpoint<TenantPageQuery, SqlSugarPagedList<TenantDto>>
{
    public override void Configure()
    {
        Get("/sys/tenant/page");
        Summary(summary => summary.Summary = "租户分页列表");
        Options(options => options.WithTags("租户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Tenant.Query));
    }

    public override async Task HandleAsync(TenantPageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
