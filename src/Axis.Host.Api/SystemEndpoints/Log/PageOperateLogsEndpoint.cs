using Axis.Application.System.Log;
using Axis.Application.System.Log.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Log;

/// <summary>
/// 分页查询操作日志。
/// </summary>
public sealed class PageOperateLogsEndpoint(ILogAppService logService)
    : AxisEndpoint<OperateLogPageQuery, SqlSugarPagedList<OperateLogDto>>
{
    public override void Configure()
    {
        Get("/sys/log/operate/page");
        Summary(s => s.Summary = "操作日志分页列表");
        Options(x => x.WithTags("日志管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.LogOperate.Query));
    }

    public override async Task HandleAsync(OperateLogPageQuery req, CancellationToken ct)
        => Response = await logService.PageOperateAsync(req, ct);
}
