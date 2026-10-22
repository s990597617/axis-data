using Axis.Application.Demo.OutboxPing;
using Axis.Application.Demo.OutboxPing.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.OutboxPing;

/// <summary>
/// 入队一条 Outbox 探测事件。
/// </summary>
public sealed class PingOutboxEndpoint(IOutboxPingAppService service)
    : AxisEndpoint<OutboxPingInput, OutboxPingDto>
{
    public override void Configure()
    {
        Post("/demo/outbox/ping");
        Summary(summary => summary.Summary = "Outbox 探测入队");
        Options(options => options.WithTags("Outbox探测")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.Brand.Query)
            .Transactional());
    }

    public override async Task HandleAsync(OutboxPingInput request, CancellationToken ct)
        => Response = await service.PingAsync(request, ct);
}
