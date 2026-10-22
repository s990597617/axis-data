using Axis.Application.DataHub.Source;
using Axis.Application.DataHub.Source.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.Source;

/// <summary>
/// 删除数据源。
/// </summary>
public sealed class DeleteSourceEndpoint(ISourceAppService service) : AxisEndpoint<SourceIdRequest>
{
    public override void Configure()
    {
        Delete("/datahub/source/{Id}");
        Summary(summary => summary.Summary = "删除数据源");
        Options(options => options.WithTags("数据源管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.Source.Delete));
    }

    public override async Task HandleAsync(SourceIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
