using Axis.Application.DataHub.Common;
using Axis.Application.DataHub.Dataset;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.Dataset;

/// <summary>
/// 数据集下拉选项。
/// </summary>
public sealed class ListDatasetLookupEndpoint(IDatasetAppService service)
    : AxisEndpointWithoutRequest<IReadOnlyList<DataHubLookupItemDto>>
{
    public override void Configure()
    {
        Get("/datahub/dataset/lookup");
        Summary(summary => summary.Summary = "数据集下拉选项");
        Options(options => options.WithTags("数据集管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.Dataset.Query));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.ListLookupAsync(ct);
}
