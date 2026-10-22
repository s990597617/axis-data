using Axis.Application.DataHub.IngestRun;
using Axis.Application.DataHub.IngestRun.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.IngestRun;

/// <summary>
/// 分页查询采集运行。
/// </summary>
public sealed class PageIngestRunsEndpoint(IIngestRunAppService service)
    : AxisEndpoint<IngestRunPageQuery, SqlSugarPagedList<IngestRunDto>>
{
    public override void Configure()
    {
        Get("/datahub/ingest-run/page");
        Summary(summary => summary.Summary = "采集运行分页列表");
        Options(options => options.WithTags("采集运行管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.IngestRun.Query));
    }

    public override async Task HandleAsync(IngestRunPageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
