using Axis.Application.System.Org;
using Axis.Application.System.Org.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Attributes;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Org;

/// <summary>
/// 获取启用状态的机构树。
/// </summary>
/// <remarks>
/// 归属机构模块；用户管理页分配机构时也可调用，故允许机构查询或用户查询权限之一。
/// </remarks>
public sealed class GetOrgTreeEndpoint(IOrgAppService service)
    : AxisEndpointWithoutRequest<IReadOnlyList<OrgTreeNode>>
{
    public override void Configure()
    {
        Get("/sys/org/tree");
        Summary(s => s.Summary = "获取机构树");
        Options(x => x.WithTags("机构管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(
                PermissionPolicy.AnyOf,
                SystemPermissionCodes.Org.Query,
                SystemPermissionCodes.User.Query));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.GetTreeAsync(ct);
}
