using Axis.Application.DataHub.Common;
using Axis.Application.DataHub.Source;
using Axis.Application.DataHub.Source.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.Source;

/// <summary>
/// 列出数据源对象的列，供数据集表单选择主键/增量字段。
/// </summary>
public sealed class ListSourceColumnsEndpoint(ISourceSchemaAppService service)
    : AxisEndpoint<ListSourceColumnsRequest, IReadOnlyList<DataHubLookupItemDto>>
{
    public override void Configure()
    {
        Get("/datahub/source/{Id}/columns");
        Summary(summary => summary.Summary = "数据源列列表");
        Options(options => options.WithTags("数据源管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.Source.Query));
    }

    public override async Task HandleAsync(ListSourceColumnsRequest request, CancellationToken ct)
        => Response = await service.ListColumnsAsync(request.Id, request.ObjectName, ct);
}