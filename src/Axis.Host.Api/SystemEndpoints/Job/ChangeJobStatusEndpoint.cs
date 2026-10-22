using Axis.Application.System.Job;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Job;

/// <summary>
/// 启停定时任务请求。
/// </summary>
public sealed class ChangeJobStatusRequest
{
    public long Id { get; set; }
    public bool Enabled { get; set; }
}

/// <summary>
/// 启用或停用定时任务。
/// </summary>
public sealed class ChangeJobStatusEndpoint(IJobAppService service) : AxisEndpoint<ChangeJobStatusRequest>
{
    public override void Configure()
    {
        Put("/sys/job/{Id}/status");
        Summary(s => s.Summary = "启用或停用定时任务");
        Options(o => o.WithTags("定时任务")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Job.Edit));
    }

    public override async Task HandleAsync(ChangeJobStatusRequest request, CancellationToken ct)
        => await service.ChangeStatusAsync(request.Id, request.Enabled, ct);
}
