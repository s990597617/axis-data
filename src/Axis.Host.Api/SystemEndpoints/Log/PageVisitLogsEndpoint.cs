using Axis.Application.System.Log;
using Axis.Application.System.Log.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Log;

/// <summary>
/// 分页查询访问日志。
/// </summary>
public sealed class PageVisitLogsEndpoint(ILogAppService logService)
    : AxisEndpoint<VisitLogPageQuery, SqlSugarPagedList<VisitLogDto>>
{
    public override void Configure()
    {
        Get("/sys/log/visit/page");
        Summary(s => s.Summary = "访问日志分页列表");
        Options(x => x.WithTags("日志管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.LogVisit.Query));
    }

    public override async Task HandleAsync(VisitLogPageQuery req, CancellationToken ct)
        => Response = await logService.PageVisitAsync(req, ct);
}
