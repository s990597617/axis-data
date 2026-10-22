using Axis.Application.DataHub.IngestJob;
using Axis.Application.DataHub.IngestJob.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.IngestJob;

/// <summary>
/// 分页查询采集任务。
/// </summary>
public sealed class PageIngestJobsEndpoint(IIngestJobAppService service)
    : AxisEndpoint<IngestJobPageQuery, SqlSugarPagedList<IngestJobDto>>
{
    public override void Configure()
    {
        Get("/datahub/ingest-job/page");
        Summary(summary => summary.Summary = "采集任务分页列表");
        Options(options => options.WithTags("采集任务管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.IngestJob.Query));
    }

    public override async Task HandleAsync(IngestJobPageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
