using Axis.Application.System.OpenApiIdentity;
using Axis.Application.System.OpenApiIdentity.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.OpenApiIdentity;

/// <summary>
/// 创建开放接口身份。
/// </summary>
public sealed class CreateOpenApiIdentityEndpoint(IOpenApiIdentityAppService service)
    : AxisEndpoint<SaveOpenApiIdentityInput, CreateOpenApiIdentityResult>
{
    public override void Configure()
    {
        Post("/sys/open-api-identity");
        Summary(s => s.Summary = "创建开放接口身份");
        Options(x => x.WithTags("开放接口身份")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.OpenApiIdentity.Create)
            .Transactional());
    }

    public override async Task HandleAsync(SaveOpenApiIdentityInput request, CancellationToken ct)
        => Response = await service.CreateAsync(request, ct);
}
