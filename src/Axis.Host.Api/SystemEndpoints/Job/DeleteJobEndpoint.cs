using Axis.Application.System.Job;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Job;

/// <summary>
/// 删除定时任务请求。
/// </summary>
public sealed class DeleteJobRequest
{
    public long Id { get; set; }
}

/// <summary>
/// 删除定时任务。
/// </summary>
public sealed class DeleteJobEndpoint(IJobAppService service) : AxisEndpoint<DeleteJobRequest>
{
    public override void Configure()
    {
        Delete("/sys/job/{Id}");
        Summary(s => s.Summary = "删除定时任务");
        Options(o => o.WithTags("定时任务")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Job.Delete));
    }

    public override async Task HandleAsync(DeleteJobRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
