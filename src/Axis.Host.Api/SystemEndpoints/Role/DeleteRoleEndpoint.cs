using Axis.Application.System.Role;
using Axis.Application.System.Role.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Role;

/// <summary>
/// 删除角色及其关联授权。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Role.Delete"/> 权限；在事务中执行。
/// </remarks>
public sealed class DeleteRoleEndpoint(IRoleAppService service) : AxisEndpoint<RoleIdRequest>
{
    public override void Configure()
    {
        Delete("/sys/role/{Id}");
        Summary(summary => summary.Summary = "删除角色");
        Options(options => options.WithTags("角色管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Role.Delete)
            .Transactional());
    }

    public override async Task HandleAsync(RoleIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
