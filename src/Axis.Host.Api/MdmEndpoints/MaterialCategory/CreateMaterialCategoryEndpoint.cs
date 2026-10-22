using Axis.Application.Mdm.MaterialCategory;
using Axis.Application.Mdm.MaterialCategory.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.MaterialCategory;

/// <summary>
/// 新增物料分类。
/// </summary>
/// <remarks>
/// 需要 <see cref="MdmPermissionCodes.MaterialCategory.Create"/> 权限。
/// </remarks>
public sealed class CreateMaterialCategoryEndpoint(IMaterialCategoryAppService service)
    : AxisEndpoint<SaveMaterialCategoryInput>
{
    public override void Configure()
    {
        Post("/mdm/material-category");
        Summary(summary => summary.Summary = "新增物料分类");
        Options(options => options.WithTags("物料分类")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.MaterialCategory.Create)
            .Transactional());
    }

    public override async Task HandleAsync(SaveMaterialCategoryInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
