using Axis.Application.Mdm.MaterialCategory;
using Axis.Application.Mdm.MaterialCategory.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Attributes;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.MaterialCategory;

/// <summary>
/// 获取启用状态的物料分类树。
/// </summary>
/// <remarks>
/// 归属分类模块；物料维护页筛选与选分类也可调用，故允许分类查询或物料查询权限之一。
/// </remarks>
public sealed class GetMaterialCategoryTreeEndpoint(IMaterialCategoryAppService service)
    : AxisEndpointWithoutRequest<IReadOnlyList<MaterialCategoryTreeNode>>
{
    public override void Configure()
    {
        Get("/mdm/material-category/tree");
        Summary(s => s.Summary = "获取物料分类树");
        Options(x => x.WithTags("物料分类")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(
                PermissionPolicy.AnyOf,
                MdmPermissionCodes.MaterialCategory.Query,
                MdmPermissionCodes.Material.Query));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.GetTreeAsync(ct);
}
