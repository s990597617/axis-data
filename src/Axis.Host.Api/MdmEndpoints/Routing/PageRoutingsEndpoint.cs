using Axis.Application.Mdm.Routing;
using Axis.Application.Mdm.Routing.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Routing;

/// <summary>
/// 分页查询工艺路线。
/// </summary>
public sealed class PageRoutingsEndpoint(IRoutingAppService service)
    : AxisEndpoint<RoutingPageQuery, SqlSugarPagedList<RoutingDto>>
{
    public override void Configure()
    {
        Get("/mdm/routing/page");
        Summary(summary => summary.Summary = "工艺路线分页列表");
        Options(options => options.WithTags("工艺路线")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.Routing.Query));
    }

    public override async Task HandleAsync(RoutingPageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
