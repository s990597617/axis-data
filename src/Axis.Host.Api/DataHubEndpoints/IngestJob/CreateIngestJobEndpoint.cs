using Axis.Application.DataHub.IngestJob;
using Axis.Application.DataHub.IngestJob.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.IngestJob;

/// <summary>
/// 新增采集任务。
/// </summary>
public sealed class CreateIngestJobEndpoint(IIngestJobAppService service) : AxisEndpoint<SaveIngestJobInput>
{
    public override void Configure()
    {
        Post("/datahub/ingest-job");
        Summary(summary => summary.Summary = "新增采集任务");
        Options(options => options.WithTags("采集任务管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.IngestJob.Create));
    }

    public override async Task HandleAsync(SaveIngestJobInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
