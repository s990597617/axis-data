using Axis.Application.System.Monitor;
using Axis.Application.System.Monitor.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Monitor;

/// <summary>
/// 获取服务监控快照（CPU / 内存 / 磁盘 / 运行时）。
/// </summary>
public sealed class GetServerMonitorEndpoint(IServerMonitorAppService service)
    : AxisEndpointWithoutRequest<ServerMonitorDto>
{
    public override void Configure()
    {
        Get("/sys/monitor");
        Summary(s => s.Summary = "获取服务监控信息");
        Options(x => x.WithTags("服务监控")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Monitor.Query));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.GetAsync(ct);
}
