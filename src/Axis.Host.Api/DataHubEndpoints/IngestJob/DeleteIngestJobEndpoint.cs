using Axis.Application.DataHub.IngestJob;
using Axis.Application.DataHub.IngestJob.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.IngestJob;

/// <summary>
/// 删除采集任务。
/// </summary>
public sealed class DeleteIngestJobEndpoint(IIngestJobAppService service) : AxisEndpoint<IngestJobIdRequest>
{
    public override void Configure()
    {
        Delete("/datahub/ingest-job/{Id}");
        Summary(summary => summary.Summary = "删除采集任务");
        Options(options => options.WithTags("采集任务管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.IngestJob.Delete));
    }

    public override async Task HandleAsync(IngestJobIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
