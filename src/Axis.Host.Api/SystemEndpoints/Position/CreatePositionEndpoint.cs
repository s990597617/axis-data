using Axis.Application.System.Position;
using Axis.Application.System.Position.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Position;

/// <summary>
/// 新增岗位。
/// </summary>
public sealed class CreatePositionEndpoint(IPositionAppService service) : AxisEndpoint<SavePositionInput>
{
    public override void Configure()
    {
        Post("/sys/pos");
        Summary(summary => summary.Summary = "新增岗位");
        Options(options => options.WithTags("岗位管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Position.Create));
    }

    public override async Task HandleAsync(SavePositionInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
