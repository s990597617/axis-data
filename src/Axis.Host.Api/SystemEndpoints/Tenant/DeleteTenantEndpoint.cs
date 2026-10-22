using Axis.Application.System.Tenant;
using Axis.Application.System.Tenant.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Tenant;

/// <summary>
/// 删除租户。
/// </summary>
public sealed class DeleteTenantEndpoint(ITenantAppService service) : AxisEndpoint<TenantIdRequest>
{
    public override void Configure()
    {
        Delete("/sys/tenant/{Id}");
        Summary(summary => summary.Summary = "删除租户");
        Options(options => options.WithTags("租户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Tenant.Delete));
    }

    public override async Task HandleAsync(TenantIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
