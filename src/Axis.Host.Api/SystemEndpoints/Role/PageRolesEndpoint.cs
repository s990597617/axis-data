using Axis.Application.System.Role;
using Axis.Application.System.Role.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Role;

/// <summary>
/// 分页查询角色列表。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Role.Query"/> 权限。
/// </remarks>
public sealed class PageRolesEndpoint(IRoleAppService service)
    : AxisEndpoint<RolePageQuery, SqlSugarPagedList<RoleDto>>
{
    public override void Configure()
    {
        Get("/sys/role/page");
        Summary(summary => summary.Summary = "角色分页列表");
        Options(options => options.WithTags("角色管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Role.Query));
    }

    public override async Task HandleAsync(RolePageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
