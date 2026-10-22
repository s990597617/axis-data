using Axis.Application.DataHub.Dataset;
using Axis.Application.DataHub.Dataset.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.Dataset;

/// <summary>
/// 更新数据集。
/// </summary>
public sealed class UpdateDatasetEndpoint(IDatasetAppService service) : AxisEndpoint<SaveDatasetInput>
{
    public override void Configure()
    {
        Put("/datahub/dataset/{Id}");
        Summary(summary => summary.Summary = "更新数据集");
        Options(options => options.WithTags("数据集管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.Dataset.Edit));
    }

    public override async Task HandleAsync(SaveDatasetInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
