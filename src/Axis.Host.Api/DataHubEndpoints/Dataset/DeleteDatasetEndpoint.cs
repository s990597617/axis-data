using Axis.Application.DataHub.Dataset;
using Axis.Application.DataHub.Dataset.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.Dataset;

/// <summary>
/// 删除数据集。
/// </summary>
public sealed class DeleteDatasetEndpoint(IDatasetAppService service) : AxisEndpoint<DatasetIdRequest>
{
    public override void Configure()
    {
        Delete("/datahub/dataset/{Id}");
        Summary(summary => summary.Summary = "删除数据集");
        Options(options => options.WithTags("数据集管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.Dataset.Delete));
    }

    public override async Task HandleAsync(DatasetIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
