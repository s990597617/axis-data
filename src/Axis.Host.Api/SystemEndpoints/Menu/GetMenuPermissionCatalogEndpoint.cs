using Axis.Application.System.Menu;
using Axis.Application.System.Menu.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Attributes;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Menu;

/// <summary>
/// 获取系统权限码目录，供菜单按钮表单级联选择。
/// </summary>
/// <remarks>
/// 需要菜单查询、新增或编辑权限之一。
/// </remarks>
public sealed class GetMenuPermissionCatalogEndpoint(IMenuAppService service)
    : AxisEndpointWithoutRequest<IReadOnlyList<PermissionCatalogModuleDto>>
{
    public override void Configure()
    {
        Get("/sys/menu/permission-catalog");
        Summary(summary =>
        {
            summary.Summary = "权限码目录";
            summary.Description = "返回按模块分组的系统权限码，数据来自 SystemPermissionCodes 元数据扫描";
        });
        Options(options => options.WithTags("菜单管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(
                PermissionPolicy.AnyOf,
                SystemPermissionCodes.Menu.Query,
                SystemPermissionCodes.Menu.Create,
                SystemPermissionCodes.Menu.Edit));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.GetPermissionCatalogAsync(ct);
}
