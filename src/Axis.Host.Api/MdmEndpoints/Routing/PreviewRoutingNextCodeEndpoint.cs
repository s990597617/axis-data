using Axis.Application.Mdm.Routing;
using Axis.Application.Mdm.Routing.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Routing;

/// <summary>
/// 预览工艺路线下一编码（不占用流水）。
/// </summary>
public sealed class PreviewRoutingNextCodeEndpoint(IRoutingAppService service)
    : AxisEndpointWithoutRequest<RoutingNextCodeDto>
{
    public override void Configure()
    {
        Get("/mdm/routing/next-code");
        Summary(summary => summary.Summary = "预览工艺路线下一编码");
        Options(options => options.WithTags("工艺路线")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.Routing.Create));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.PreviewNextCodeAsync(ct);
}
