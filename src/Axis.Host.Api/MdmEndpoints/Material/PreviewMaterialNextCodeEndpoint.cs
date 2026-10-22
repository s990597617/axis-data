using Axis.Application.Mdm.Material;
using Axis.Application.Mdm.Material.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Material;

/// <summary>
/// 预览物料下一编码（不占用流水）。
/// </summary>
public sealed class PreviewMaterialNextCodeEndpoint(IMaterialAppService service)
    : AxisEndpointWithoutRequest<MaterialNextCodeDto>
{
    public override void Configure()
    {
        Get("/mdm/material/next-code");
        Summary(summary => summary.Summary = "预览物料档案下一编码");
        Options(options => options.WithTags("物料档案")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.Material.Create));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.PreviewNextCodeAsync(ct);
}
