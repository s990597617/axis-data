using Axis.Application.System.OpenApiIdentity;
using Axis.Application.System.OpenApiIdentity.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.OpenApiIdentity;

/// <summary>
/// 分页查询开放接口身份。
/// </summary>
public sealed class PageOpenApiIdentitiesEndpoint(IOpenApiIdentityAppService service)
    : AxisEndpoint<OpenApiIdentityPageQuery, SqlSugarPagedList<OpenApiIdentityDto>>
{
    public override void Configure()
    {
        Get("/sys/open-api-identity/page");
        Summary(s => s.Summary = "开放接口身份分页");
        Options(x => x.WithTags("开放接口身份")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.OpenApiIdentity.Query));
    }

    public override async Task HandleAsync(OpenApiIdentityPageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
