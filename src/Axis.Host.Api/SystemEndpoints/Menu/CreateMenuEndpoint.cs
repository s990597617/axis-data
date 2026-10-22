using Axis.Application.System.Menu;
using Axis.Application.System.Menu.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Menu;

/// <summary>
/// 新增菜单、目录或按钮。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Menu.Create"/> 权限。
/// </remarks>
public sealed class CreateMenuEndpoint(IMenuAppService service) : AxisEndpoint<SaveMenuInput>
{
    public override void Configure()
    {
        Post("/sys/menu");
        Summary(summary => summary.Summary = "新增菜单节点");
        Options(options => options.WithTags("菜单管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Menu.Create));
    }

    public override async Task HandleAsync(SaveMenuInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
