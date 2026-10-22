using Axis.Application.System.Tenant;
using Axis.Application.System.Tenant.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Tenant;

/// <summary>
/// 新增租户。
/// </summary>
public sealed class CreateTenantEndpoint(ITenantAppService service) : AxisEndpoint<SaveTenantInput>
{
    public override void Configure()
    {
        Post("/sys/tenant");
        Summary(summary => summary.Summary = "新增租户");
        Options(options => options.WithTags("租户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Tenant.Create)
            .Transactional());
    }

    public override async Task HandleAsync(SaveTenantInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
