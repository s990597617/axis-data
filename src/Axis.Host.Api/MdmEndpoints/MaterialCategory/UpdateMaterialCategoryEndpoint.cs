using Axis.Application.Mdm.MaterialCategory;
using Axis.Application.Mdm.MaterialCategory.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.MaterialCategory;

/// <summary>
/// 更新物料分类。
/// </summary>
/// <remarks>
/// 需要 <see cref="MdmPermissionCodes.MaterialCategory.Edit"/> 权限；路由绑定 <c>Id</c>。
/// </remarks>
public sealed class UpdateMaterialCategoryEndpoint(IMaterialCategoryAppService service)
    : AxisEndpoint<SaveMaterialCategoryInput>
{
    public override void Configure()
    {
        Put("/mdm/material-category/{Id}");
        Summary(summary => summary.Summary = "更新物料分类");
        Options(options => options.WithTags("物料分类")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.MaterialCategory.Edit)
            .Transactional());
    }

    public override async Task HandleAsync(SaveMaterialCategoryInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
