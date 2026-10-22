using Axis.Application.System.Position;
using Axis.Application.System.Position.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Position;

/// <summary>
/// 更新岗位。
/// </summary>
public sealed class UpdatePositionEndpoint(IPositionAppService service) : AxisEndpoint<SavePositionInput>
{
    public override void Configure()
    {
        Put("/sys/pos/{Id}");
        Summary(summary => summary.Summary = "更新岗位");
        Options(options => options.WithTags("岗位管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Position.Edit));
    }

    public override async Task HandleAsync(SavePositionInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
