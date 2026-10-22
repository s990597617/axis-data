using Axis.Application.System.Log;
using Axis.Application.System.Log.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Log;

/// <summary>
/// 分页查询异常日志。
/// </summary>
public sealed class PageErrorLogsEndpoint(ILogAppService logService)
    : AxisEndpoint<ErrorLogPageQuery, SqlSugarPagedList<ErrorLogDto>>
{
    public override void Configure()
    {
        Get("/sys/log/error/page");
        Summary(s => s.Summary = "异常日志分页列表");
        Options(x => x.WithTags("日志管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.LogError.Query));
    }

    public override async Task HandleAsync(ErrorLogPageQuery req, CancellationToken ct)
        => Response = await logService.PageErrorAsync(req, ct);
}
