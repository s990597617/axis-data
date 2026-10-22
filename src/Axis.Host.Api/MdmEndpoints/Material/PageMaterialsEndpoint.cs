using Axis.Application.Mdm.Material;
using Axis.Application.Mdm.Material.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Material;

/// <summary>
/// 分页查询物料。
/// </summary>
public sealed class PageMaterialsEndpoint(IMaterialAppService service)
    : AxisEndpoint<MaterialPageQuery, SqlSugarPagedList<MaterialDto>>
{
    public override void Configure()
    {
        Get("/mdm/material/page");
        Summary(summary => summary.Summary = "物料档案分页列表");
        Options(options => options.WithTags("物料档案")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.Material.Query));
    }

    public override async Task HandleAsync(MaterialPageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
