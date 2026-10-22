using Axis.Application.DataHub.IngestRun;
using Axis.Application.DataHub.IngestRun.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.IngestRun;

/// <summary>
/// 更新采集运行。
/// </summary>
public sealed class UpdateIngestRunEndpoint(IIngestRunAppService service) : AxisEndpoint<SaveIngestRunInput>
{
    public override void Configure()
    {
        Put("/datahub/ingest-run/{Id}");
        Summary(summary => summary.Summary = "更新采集运行");
        Options(options => options.WithTags("采集运行管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.IngestRun.Edit));
    }

    public override async Task HandleAsync(SaveIngestRunInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
