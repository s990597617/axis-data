using Axis.Application.System.Position;
using Axis.Application.System.Position.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Position;

/// <summary>
/// 分页查询岗位。
/// </summary>
public sealed class PagePositionsEndpoint(IPositionAppService service)
    : AxisEndpoint<PositionPageQuery, SqlSugarPagedList<PositionDto>>
{
    public override void Configure()
    {
        Get("/sys/pos/page");
        Summary(summary => summary.Summary = "岗位分页列表");
        Options(options => options.WithTags("岗位管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Position.Query));
    }

    public override async Task HandleAsync(PositionPageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
