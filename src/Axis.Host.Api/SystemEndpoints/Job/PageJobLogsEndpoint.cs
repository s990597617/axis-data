using Axis.Application.System.Job;
using Axis.Application.System.Job.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Job;

/// <summary>
/// 定时任务执行日志分页。
/// </summary>
public sealed class PageJobLogsEndpoint(IJobAppService service)
    : AxisEndpoint<JobLogPageQuery, SqlSugarPagedList<JobLogDto>>
{
    public override void Configure()
    {
        Get("/sys/job/log/page");
        Summary(s => s.Summary = "定时任务执行日志分页");
        Options(o => o.WithTags("定时任务")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Job.Log));
    }

    public override async Task HandleAsync(JobLogPageQuery request, CancellationToken ct)
        => Response = await service.PageLogsAsync(request, ct);
}
