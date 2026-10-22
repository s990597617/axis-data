using Axis.Application.System.Org;
using Axis.Application.System.Org.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Org;

/// <summary>
/// 新增机构。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Org.Create"/> 权限。
/// </remarks>
public sealed class CreateOrgEndpoint(IOrgAppService service) : AxisEndpoint<SaveOrgInput>
{
    public override void Configure()
    {
        Post("/sys/org");
        Summary(summary => summary.Summary = "新增机构");
        Options(options => options.WithTags("机构管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Org.Create));
    }

    public override async Task HandleAsync(SaveOrgInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
