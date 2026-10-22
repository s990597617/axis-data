using Axis.Application.Mdm.Material;
using Axis.Application.Mdm.Material.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Material;

/// <summary>
/// 更新物料。
/// </summary>
public sealed class UpdateMaterialEndpoint(IMaterialAppService service) : AxisEndpoint<SaveMaterialInput>
{
    public override void Configure()
    {
        Put("/mdm/material/{Id}");
        Summary(summary => summary.Summary = "更新物料档案");
        Options(options => options.WithTags("物料档案")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.Material.Edit)
            .Transactional());
    }

    public override async Task HandleAsync(SaveMaterialInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
