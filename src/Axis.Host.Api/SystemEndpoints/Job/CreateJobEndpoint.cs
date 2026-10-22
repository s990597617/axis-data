using Axis.Application.System.Job;
using Axis.Application.System.Job.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Job;

/// <summary>
/// 新增定时任务。
/// </summary>
public sealed class CreateJobEndpoint(IJobAppService service) : AxisEndpoint<SaveJobInput>
{
    public override void Configure()
    {
        Post("/sys/job");
        Summary(s => s.Summary = "新增定时任务");
        Options(o => o.WithTags("定时任务")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Job.Create));
    }

    public override async Task HandleAsync(SaveJobInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
