using Axis.Application.Mdm.Routing;
using Axis.Application.Mdm.Routing.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Routing;

/// <summary>
/// 获取工艺路线详情（含工序行）。
/// </summary>
public sealed class GetRoutingEndpoint(IRoutingAppService service)
    : AxisEndpoint<RoutingIdRequest, RoutingDto>
{
    public override void Configure()
    {
        Get("/mdm/routing/{Id}");
        Summary(summary => summary.Summary = "工艺路线详情");
        Options(options => options.WithTags("工艺路线")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.Routing.Query));
    }

    public override async Task HandleAsync(RoutingIdRequest request, CancellationToken ct)
        => Response = await service.GetDetailAsync(request.Id, ct);
}
