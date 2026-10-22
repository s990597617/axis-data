using Axis.Application.DataHub.Source;
using Axis.Application.DataHub.Source.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.Source;

/// <summary>
/// 分页查询数据源。
/// </summary>
public sealed class PageSourcesEndpoint(ISourceAppService service)
    : AxisEndpoint<SourcePageQuery, SqlSugarPagedList<SourceDto>>
{
    public override void Configure()
    {
        Get("/datahub/source/page");
        Summary(summary => summary.Summary = "数据源分页列表");
        Options(options => options.WithTags("数据源管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.Source.Query));
    }

    public override async Task HandleAsync(SourcePageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
