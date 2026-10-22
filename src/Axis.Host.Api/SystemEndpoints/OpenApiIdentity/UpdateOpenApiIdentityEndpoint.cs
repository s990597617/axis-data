using Axis.Application.System.OpenApiIdentity;
using Axis.Application.System.OpenApiIdentity.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.OpenApiIdentity;

/// <summary>
/// 更新开放接口身份。
/// </summary>
public sealed class UpdateOpenApiIdentityEndpoint(IOpenApiIdentityAppService service)
    : AxisEndpoint<SaveOpenApiIdentityInput>
{
    public override void Configure()
    {
        Put("/sys/open-api-identity/{id}");
        Summary(s => s.Summary = "更新开放接口身份");
        Options(x => x.WithTags("开放接口身份")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.OpenApiIdentity.Edit)
            .Transactional());
    }

    public override async Task HandleAsync(SaveOpenApiIdentityInput request, CancellationToken ct)
    {
        var id = Route<long>("id");
        await service.UpdateAsync(id, request, ct);
    }
}
