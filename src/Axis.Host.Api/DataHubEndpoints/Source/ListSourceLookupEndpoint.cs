using Axis.Application.DataHub.Common;
using Axis.Application.DataHub.Source;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.Source;

/// <summary>
/// 数据源下拉选项。
/// </summary>
public sealed class ListSourceLookupEndpoint(ISourceAppService service)
    : AxisEndpointWithoutRequest<IReadOnlyList<DataHubLookupItemDto>>
{
    public override void Configure()
    {
        Get("/datahub/source/lookup");
        Summary(summary => summary.Summary = "数据源下拉选项");
        Options(options => options.WithTags("数据源管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.Source.Query));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.ListLookupAsync(ct);
}
