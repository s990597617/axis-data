using Axis.Application.Mdm.Material;
using Axis.Application.Mdm.Material.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Material;

/// <summary>
/// 删除物料。
/// </summary>
public sealed class DeleteMaterialEndpoint(IMaterialAppService service) : AxisEndpoint<MaterialIdRequest>
{
    public override void Configure()
    {
        Delete("/mdm/material/{Id}");
        Summary(summary => summary.Summary = "删除物料档案");
        Options(options => options.WithTags("物料档案")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.Material.Delete));
    }

    public override async Task HandleAsync(MaterialIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
