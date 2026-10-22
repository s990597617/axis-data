using Axis.Application.Mdm.MaterialCategory;
using Axis.Application.Mdm.MaterialCategory.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.MaterialCategory;

/// <summary>
/// 删除物料分类。
/// </summary>
/// <remarks>
/// 需要 <see cref="MdmPermissionCodes.MaterialCategory.Delete"/> 权限。
/// </remarks>
public sealed class DeleteMaterialCategoryEndpoint(IMaterialCategoryAppService service)
    : AxisEndpoint<MaterialCategoryIdRequest>
{
    public override void Configure()
    {
        Delete("/mdm/material-category/{Id}");
        Summary(summary => summary.Summary = "删除物料分类");
        Options(options => options.WithTags("物料分类")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.MaterialCategory.Delete));
    }

    public override async Task HandleAsync(MaterialCategoryIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
