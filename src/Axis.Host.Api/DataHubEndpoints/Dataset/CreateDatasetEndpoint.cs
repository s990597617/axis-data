using Axis.Application.DataHub.Dataset;
using Axis.Application.DataHub.Dataset.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.Dataset;

/// <summary>
/// 新增数据集。
/// </summary>
public sealed class CreateDatasetEndpoint(IDatasetAppService service) : AxisEndpoint<SaveDatasetInput>
{
    public override void Configure()
    {
        Post("/datahub/dataset");
        Summary(summary => summary.Summary = "新增数据集");
        Options(options => options.WithTags("数据集管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.Dataset.Create));
    }

    public override async Task HandleAsync(SaveDatasetInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
