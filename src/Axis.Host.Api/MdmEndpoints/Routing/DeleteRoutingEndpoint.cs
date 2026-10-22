using Axis.Application.Mdm.Routing;
using Axis.Application.Mdm.Routing.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Routing;

/// <summary>
/// 删除工艺路线及其工序行。
/// </summary>
public sealed class DeleteRoutingEndpoint(IRoutingAppService service) : AxisEndpoint<RoutingIdRequest>
{
    public override void Configure()
    {
        Delete("/mdm/routing/{Id}");
        Summary(summary => summary.Summary = "删除工艺路线");
        Options(options => options.WithTags("工艺路线")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.Routing.Delete)
            .Transactional());
    }

    public override async Task HandleAsync(RoutingIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
