using Axis.Application.System.OpenApiIdentity;
using Axis.Application.System.OpenApiIdentity.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.OpenApiIdentity;

/// <summary>
/// 重置开放接口 AccessSecret。
/// </summary>
public sealed class ResetOpenApiSecretEndpoint(IOpenApiIdentityAppService service)
    : AxisEndpointWithoutRequest<ResetOpenApiSecretResult>
{
    public override void Configure()
    {
        Post("/sys/open-api-identity/{id}/reset-secret");
        Summary(s => s.Summary = "重置开放接口密钥");
        Options(x => x.WithTags("开放接口身份")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.OpenApiIdentity.ResetSecret)
            .Transactional());
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<long>("id");
        Response = await service.ResetSecretAsync(id, ct);
    }
}
