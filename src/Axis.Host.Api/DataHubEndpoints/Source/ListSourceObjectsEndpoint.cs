using Axis.Application.DataHub.Common;
using Axis.Application.DataHub.Source;
using Axis.Application.DataHub.Source.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.Source;

/// <summary>
/// 列出数据源中的表或视图，供数据集表单选择对象名。
/// </summary>
public sealed class ListSourceObjectsEndpoint(ISourceSchemaAppService service)
    : AxisEndpoint<ListSourceObjectsRequest, IReadOnlyList<DataHubLookupItemDto>>
{
    public override void Configure()
    {
        Get("/datahub/source/{Id}/objects");
        Summary(summary => summary.Summary = "数据源对象列表");
        Options(options => options.WithTags("数据源管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.Source.Query));
    }

    public override async Task HandleAsync(ListSourceObjectsRequest request, CancellationToken ct)
        => Response = await service.ListObjectsAsync(request.Id, request.ObjectType, ct);
}