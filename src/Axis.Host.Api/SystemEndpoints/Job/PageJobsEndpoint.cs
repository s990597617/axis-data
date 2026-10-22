using Axis.Application.System.Job;
using Axis.Application.System.Job.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Job;

/// <summary>
/// 定时任务分页列表。
/// </summary>
public sealed class PageJobsEndpoint(IJobAppService service)
    : AxisEndpoint<JobPageQuery, SqlSugarPagedList<JobDto>>
{
    public override void Configure()
    {
        Get("/sys/job/page");
        Summary(s => s.Summary = "定时任务分页列表");
        Options(o => o.WithTags("定时任务")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Job.Query));
    }

    public override async Task HandleAsync(JobPageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
