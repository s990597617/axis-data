using Axis.Application.System.Log;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Log;

/// <summary>
/// 清空访问日志。
/// </summary>
public sealed class ClearVisitLogsEndpoint(ILogAppService logService)
    : AxisEndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/sys/log/visit");
        Summary(s => s.Summary = "清空访问日志");
        Options(x => x.WithTags("日志管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.LogVisit.Clear));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await logService.ClearVisitAsync(ct);
        await Send.NoContentAsync(ct);
    }
}
