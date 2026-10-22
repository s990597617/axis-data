using Axis.Application.System.Role;
using Axis.Application.System.Role.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Role;

/// <summary>
/// 查询角色详情（含已授权菜单树）。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Role.Detail"/> 权限。
/// </remarks>
public sealed class GetRoleDetailEndpoint(IRoleAppService service)
    : AxisEndpoint<RoleIdRequest, RoleDetailDto?>
{
    public override void Configure()
    {
        Get("/sys/role/{Id}");
        Summary(summary => summary.Summary = "角色详情");
        Options(options => options.WithTags("角色管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Role.Detail));
    }

    public override async Task HandleAsync(RoleIdRequest request, CancellationToken ct)
        => Response = await service.GetDetailAsync(request.Id, ct);
}
