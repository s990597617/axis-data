using Axis.Application.Mdm.Routing;
using Axis.Application.Mdm.Routing.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Routing;

/// <summary>
/// 更新工艺路线（头行同事务，先删后插工序行）。
/// </summary>
public sealed class UpdateRoutingEndpoint(IRoutingAppService service) : AxisEndpoint<SaveRoutingInput>
{
    public override void Configure()
    {
        Put("/mdm/routing/{Id}");
        Summary(summary => summary.Summary = "更新工艺路线");
        Options(options => options.WithTags("工艺路线")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.Routing.Edit)
            .Transactional());
    }

    public override async Task HandleAsync(SaveRoutingInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
