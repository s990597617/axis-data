using Axis.Application.System.Role;
using Axis.Application.System.Role.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Role;

/// <summary>
/// 覆盖保存角色的菜单与按钮授权。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Role.Grant"/> 权限；在事务中执行。
/// </remarks>
public sealed class GrantRoleMenusEndpoint(IRoleAppService service) : AxisEndpoint<GrantRoleMenusInput>
{
    public override void Configure()
    {
        Put("/sys/role/{Id}/menus");
        Summary(summary => summary.Summary = "保存角色权限");
        Options(options => options.WithTags("角色管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Role.Grant)
            .Transactional());
    }

    public override async Task HandleAsync(GrantRoleMenusInput request, CancellationToken ct)
        => await service.GrantMenusAsync(request.Id, request.MenuIds, ct);
}
