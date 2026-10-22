using Axis.Application.Demo.OutboxPing;
using Axis.Application.Demo.OutboxPing.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.OutboxPing;

/// <summary>
/// 查询 Outbox 探测事件状态。
/// </summary>
public sealed class GetOutboxPingEndpoint(IOutboxPingAppService service)
    : AxisEndpoint<OutboxPingIdRequest, OutboxPingDto>
{
    public override void Configure()
    {
        Get("/demo/outbox/{EventId}");
        Summary(summary => summary.Summary = "查询 Outbox 探测状态");
        Options(options => options.WithTags("Outbox探测")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.Brand.Query));
    }

    public override async Task HandleAsync(OutboxPingIdRequest request, CancellationToken ct)
        => Response = await service.GetAsync(request.EventId, ct);
}
