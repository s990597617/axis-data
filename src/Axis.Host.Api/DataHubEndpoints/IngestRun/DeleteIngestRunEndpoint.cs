using Axis.Application.DataHub.IngestRun;
using Axis.Application.DataHub.IngestRun.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.IngestRun;

/// <summary>
/// 删除采集运行。
/// </summary>
public sealed class DeleteIngestRunEndpoint(IIngestRunAppService service) : AxisEndpoint<IngestRunIdRequest>
{
    public override void Configure()
    {
        Delete("/datahub/ingest-run/{Id}");
        Summary(summary => summary.Summary = "删除采集运行");
        Options(options => options.WithTags("采集运行管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.IngestRun.Delete));
    }

    public override async Task HandleAsync(IngestRunIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
