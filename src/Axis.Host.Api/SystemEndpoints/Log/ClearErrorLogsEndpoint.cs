using Axis.Application.System.Log;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Log;

/// <summary>
/// 清空异常日志。
/// </summary>
public sealed class ClearErrorLogsEndpoint(ILogAppService logService)
    : AxisEndpointWithoutRequest
{
    public override void Configure()
    {
        Delete("/sys/log/error");
        Summary(s => s.Summary = "清空异常日志");
        Options(x => x.WithTags("日志管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.LogError.Clear));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await logService.ClearErrorAsync(ct);
        await Send.NoContentAsync(ct);
    }
}
