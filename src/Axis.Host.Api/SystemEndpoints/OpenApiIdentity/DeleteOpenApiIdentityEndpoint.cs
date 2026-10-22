using Axis.Application.System.OpenApiIdentity;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.OpenApiIdentity;

/// <summary>
/// 删除开放接口身份。
/// </summary>
public sealed class DeleteOpenApiIdentityEndpoint(IOpenApiIdentityAppService service)
    : AxisEndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/sys/open-api-identity/{id}");
        Summary(s => s.Summary = "删除开放接口身份");
        Options(x => x.WithTags("开放接口身份")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.OpenApiIdentity.Delete)
            .Transactional());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<long>("id");
        await service.DeleteAsync(id, ct);
    }
}
