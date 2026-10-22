using Axis.Application.Mdm.Routing;
using Axis.Application.Mdm.Routing.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Routing;

/// <summary>
/// 新增工艺路线（头行同事务）。
/// </summary>
public sealed class CreateRoutingEndpoint(IRoutingAppService service) : AxisEndpoint<SaveRoutingInput>
{
    public override void Configure()
    {
        Post("/mdm/routing");
        Summary(summary => summary.Summary = "新增工艺路线");
        Options(options => options.WithTags("工艺路线")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.Routing.Create)
            .Transactional());
    }

    public override async Task HandleAsync(SaveRoutingInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
