using Axis.Application.System.Tenant;
using Axis.Application.System.Tenant.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Tenant;

/// <summary>
/// 更新租户。
/// </summary>
public sealed class UpdateTenantEndpoint(ITenantAppService service) : AxisEndpoint<SaveTenantInput>
{
    public override void Configure()
    {
        Put("/sys/tenant/{Id}");
        Summary(summary => summary.Summary = "更新租户");
        Options(options => options.WithTags("租户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Tenant.Edit)
            .Transactional());
    }

    public override async Task HandleAsync(SaveTenantInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
