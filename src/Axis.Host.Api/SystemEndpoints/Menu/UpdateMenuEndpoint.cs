using Axis.Application.System.Menu;
using Axis.Application.System.Menu.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Menu;

/// <summary>
/// 更新菜单、目录或按钮。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Menu.Edit"/> 权限；路由绑定 <c>Id</c>。
/// </remarks>
public sealed class UpdateMenuEndpoint(IMenuAppService service) : AxisEndpoint<SaveMenuInput>
{
    public override void Configure()
    {
        Put("/sys/menu/{Id}");
        Summary(summary => summary.Summary = "更新菜单节点");
        Options(options => options.WithTags("菜单管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Menu.Edit));
    }

    public override async Task HandleAsync(SaveMenuInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
