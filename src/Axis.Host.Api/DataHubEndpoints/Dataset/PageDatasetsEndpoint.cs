using Axis.Application.DataHub.Dataset;
using Axis.Application.DataHub.Dataset.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.Dataset;

/// <summary>
/// 分页查询数据集。
/// </summary>
public sealed class PageDatasetsEndpoint(IDatasetAppService service)
    : AxisEndpoint<DatasetPageQuery, SqlSugarPagedList<DatasetDto>>
{
    public override void Configure()
    {
        Get("/datahub/dataset/page");
        Summary(summary => summary.Summary = "数据集分页列表");
        Options(options => options.WithTags("数据集管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.Dataset.Query));
    }

    public override async Task HandleAsync(DatasetPageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
