using Axis.Application.DataHub.Source;
using Axis.Application.DataHub.Source.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.Source;

/// <summary>
/// 新增数据源。
/// </summary>
public sealed class CreateSourceEndpoint(ISourceAppService service) : AxisEndpoint<SaveSourceInput>
{
    public override void Configure()
    {
        Post("/datahub/source");
        Summary(summary => summary.Summary = "新增数据源");
        Options(options => options.WithTags("数据源管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.Source.Create));
    }

    public override async Task HandleAsync(SaveSourceInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
