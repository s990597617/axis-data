using Axis.Application.DataHub.IngestJob;
using Axis.Application.DataHub.IngestJob.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.IngestJob;

/// <summary>
/// 更新采集任务。
/// </summary>
public sealed class UpdateIngestJobEndpoint(IIngestJobAppService service) : AxisEndpoint<SaveIngestJobInput>
{
    public override void Configure()
    {
        Put("/datahub/ingest-job/{Id}");
        Summary(summary => summary.Summary = "更新采集任务");
        Options(options => options.WithTags("采集任务管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.IngestJob.Edit));
    }

    public override async Task HandleAsync(SaveIngestJobInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
