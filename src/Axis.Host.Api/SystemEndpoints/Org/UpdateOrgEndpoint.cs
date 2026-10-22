using Axis.Application.System.Org;
using Axis.Application.System.Org.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Org;

/// <summary>
/// 更新机构。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Org.Edit"/> 权限；路由绑定 <c>Id</c>。
/// </remarks>
public sealed class UpdateOrgEndpoint(IOrgAppService service) : AxisEndpoint<SaveOrgInput>
{
    public override void Configure()
    {
        Put("/sys/org/{Id}");
        Summary(summary => summary.Summary = "更新机构");
        Options(options => options.WithTags("机构管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Org.Edit));
    }

    public override async Task HandleAsync(SaveOrgInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
