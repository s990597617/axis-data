using Axis.Application.DataHub.IngestRun;
using Axis.Application.DataHub.IngestRun.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.IngestRun;

/// <summary>
/// 新增采集运行。
/// </summary>
public sealed class CreateIngestRunEndpoint(IIngestRunAppService service) : AxisEndpoint<SaveIngestRunInput>
{
    public override void Configure()
    {
        Post("/datahub/ingest-run");
        Summary(summary => summary.Summary = "新增采集运行");
        Options(options => options.WithTags("采集运行管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.IngestRun.Create));
    }

    public override async Task HandleAsync(SaveIngestRunInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
