using Axis.Application.DataHub.Common;
using Axis.Application.DataHub.IngestJob;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.IngestJob;

/// <summary>
/// 采集任务下拉选项。
/// </summary>
public sealed class ListIngestJobLookupEndpoint(IIngestJobAppService service)
    : AxisEndpointWithoutRequest<IReadOnlyList<DataHubLookupItemDto>>
{
    public override void Configure()
    {
        Get("/datahub/ingest-job/lookup");
        Summary(summary => summary.Summary = "采集任务下拉选项");
        Options(options => options.WithTags("采集任务管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.IngestJob.Query));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.ListLookupAsync(ct);
}
