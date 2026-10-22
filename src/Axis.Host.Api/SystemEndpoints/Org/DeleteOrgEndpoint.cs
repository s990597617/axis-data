using Axis.Application.System.Org;
using Axis.Application.System.Org.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Org;

/// <summary>
/// 删除机构。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Org.Delete"/> 权限。
/// </remarks>
public sealed class DeleteOrgEndpoint(IOrgAppService service) : AxisEndpoint<OrgIdRequest>
{
    public override void Configure()
    {
        Delete("/sys/org/{Id}");
        Summary(summary => summary.Summary = "删除机构");
        Options(options => options.WithTags("机构管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Org.Delete));
    }

    public override async Task HandleAsync(OrgIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
