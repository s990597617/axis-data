using Axis.Application.System.Position;
using Axis.Application.System.Position.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Position;

/// <summary>
/// 删除岗位。
/// </summary>
public sealed class DeletePositionEndpoint(IPositionAppService service) : AxisEndpoint<PositionIdRequest>
{
    public override void Configure()
    {
        Delete("/sys/pos/{Id}");
        Summary(summary => summary.Summary = "删除岗位");
        Options(options => options.WithTags("岗位管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Position.Delete));
    }

    public override async Task HandleAsync(PositionIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
