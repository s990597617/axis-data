using Axis.Application.DataHub.Source;
using Axis.Application.DataHub.Source.Dto;
using Axis.Domain.DataHub;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DataHubEndpoints.Source;

/// <summary>
/// 更新数据源。
/// </summary>
public sealed class UpdateSourceEndpoint(ISourceAppService service) : AxisEndpoint<SaveSourceInput>
{
    public override void Configure()
    {
        Put("/datahub/source/{Id}");
        Summary(summary => summary.Summary = "更新数据源");
        Options(options => options.WithTags("数据源管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DataHubPermissionCodes.Source.Edit));
    }

    public override async Task HandleAsync(SaveSourceInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
