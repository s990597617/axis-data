using Axis.Application.Mdm.Material;
using Axis.Application.Mdm.Material.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Material;

/// <summary>
/// 新增物料。
/// </summary>
public sealed class CreateMaterialEndpoint(IMaterialAppService service) : AxisEndpoint<SaveMaterialInput>
{
    public override void Configure()
    {
        Post("/mdm/material");
        Summary(summary => summary.Summary = "新增物料档案");
        Options(options => options.WithTags("物料档案")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.Material.Create)
            .Transactional());
    }

    public override async Task HandleAsync(SaveMaterialInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
