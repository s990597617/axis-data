using Axis.Application.System.Job;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Job;

/// <summary>
/// 立即执行定时任务请求。
/// </summary>
public sealed class RunJobRequest
{
    public long Id { get; set; }
}

/// <summary>
/// 立即执行定时任务。
/// </summary>
public sealed class RunJobEndpoint(IJobAppService service) : AxisEndpoint<RunJobRequest>
{
    public override void Configure()
    {
        Post("/sys/job/{Id}/run");
        Summary(s => s.Summary = "立即执行定时任务");
        Options(o => o.WithTags("定时任务")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Job.Run));
    }

    public override async Task HandleAsync(RunJobRequest request, CancellationToken ct)
        => await service.RunAsync(request.Id, ct);
}
