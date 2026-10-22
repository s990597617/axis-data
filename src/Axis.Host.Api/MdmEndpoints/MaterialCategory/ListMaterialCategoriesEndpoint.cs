using Axis.Application.Mdm.MaterialCategory;
using Axis.Application.Mdm.MaterialCategory.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.MaterialCategory;

/// <summary>
/// 查询物料分类平铺列表。
/// </summary>
/// <remarks>
/// 需要 <see cref="MdmPermissionCodes.MaterialCategory.Query"/> 权限。
/// </remarks>
public sealed class ListMaterialCategoriesEndpoint(IMaterialCategoryAppService service)
    : AxisEndpoint<MaterialCategoryListQuery, IReadOnlyList<MaterialCategoryDto>>
{
    public override void Configure()
    {
        Get("/mdm/material-category/list");
        Summary(summary => summary.Summary = "物料分类树列表");
        Options(options => options.WithTags("物料分类")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.MaterialCategory.Query));
    }

    public override async Task HandleAsync(MaterialCategoryListQuery request, CancellationToken ct)
        => Response = await service.ListAsync(request, ct);
}
