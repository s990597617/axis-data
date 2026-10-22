using Axis.Application.System.Org;
using Axis.Application.System.Org.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Org;

/// <summary>
/// 查询机构管理平铺列表。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Org.Query"/> 权限。
/// </remarks>
public sealed class ListOrgsEndpoint(IOrgAppService service)
    : AxisEndpoint<OrgListQuery, IReadOnlyList<OrgDto>>
{
    public override void Configure()
    {
        Get("/sys/org/list");
        Summary(summary => summary.Summary = "机构树列表");
        Options(options => options.WithTags("机构管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Org.Query));
    }

    public override async Task HandleAsync(OrgListQuery request, CancellationToken ct)
        => Response = await service.ListAsync(request, ct);
}
