using Axis.Application.System.Log;
using Axis.Application.System.Log.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Log;

/// <summary>
/// 分页查询登录日志。
/// </summary>
public sealed class PageLoginLogsEndpoint(ILogAppService logService)
    : AxisEndpoint<LoginLogPageQuery, SqlSugarPagedList<LoginLogDto>>
{
    public override void Configure()
    {
        Get("/sys/log/login/page");
        Summary(s => s.Summary = "登录日志分页列表");
        Options(x => x.WithTags("日志管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.LogLogin.Query));
    }

    public override async Task HandleAsync(LoginLogPageQuery req, CancellationToken ct)
        => Response = await logService.PageLoginAsync(req, ct);
}
