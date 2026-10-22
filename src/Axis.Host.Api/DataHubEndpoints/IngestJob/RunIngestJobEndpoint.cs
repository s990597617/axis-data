using Axis.Application.DataHub.IngestJob;
using Axis.Application.DataHub.IngestJob.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.IngestJob;

/// <summary>
/// 手动触发采集任务执行。
/// </summary>
public sealed class RunIngestJobEndpoint(IIngestJobAppService service)
    : AxisEndpoint<IngestJobIdRequest, TriggerIngestJobResultDto>
{
    public override void Configure()
    {
        Post("/datahub/ingest-job/{Id}/run");
        Summary(summary => summary.Summary = "立即执行采集任务");
        Options(options => options.WithTags("采集任务管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.IngestJob.Run));
    }

    public override async Task<TriggerIngestJobResultDto> HandleAsync(IngestJobIdRequest request, CancellationToken ct)
        => await service.TriggerRunAsync(request.Id, ct);
}
