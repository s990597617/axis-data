using Axis.Application.System.Online;
using Axis.Application.System.Online.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Online;

/// <summary>
/// 分页查询在线会话。
/// </summary>
public sealed class PageOnlineSessionsEndpoint(IOnlineSessionAppService service)
    : AxisEndpoint<OnlineSessionPageQuery, SqlSugarPagedList<OnlineSessionDto>>
{
    public override void Configure()
    {
        Get("/sys/online/page");
        Summary(s => s.Summary = "分页查询在线会话");
        Options(x => x.WithTags("在线用户")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Online.Query));
    }

    public override async Task HandleAsync(OnlineSessionPageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
