using Axis.Application.System.Menu;
using Axis.Application.System.Menu.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Menu;

/// <summary>
/// 删除菜单、目录或按钮。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Menu.Delete"/> 权限。
/// </remarks>
public sealed class DeleteMenuEndpoint(IMenuAppService service) : AxisEndpoint<MenuIdRequest>
{
    public override void Configure()
    {
        Delete("/sys/menu/{Id}");
        Summary(summary => summary.Summary = "删除菜单节点");
        Options(options => options.WithTags("菜单管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Menu.Delete));
    }

    public override async Task HandleAsync(MenuIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
