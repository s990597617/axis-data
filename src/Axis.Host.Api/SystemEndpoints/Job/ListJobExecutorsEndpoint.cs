using Axis.Application.System.Job;
using Axis.Application.System.Job.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Attributes;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Job;

/// <summary>
/// 获取已注册任务执行器目录，供任务表单选择。
/// </summary>
public sealed class ListJobExecutorsEndpoint(IJobAppService service)
    : AxisEndpointWithoutRequest<IReadOnlyList<JobExecutorDto>>
{
    public override void Configure()
    {
        Get("/sys/job/executors");
        Summary(summary =>
        {
            summary.Summary = "任务执行器目录";
            summary.Description = "返回 DI 中已注册的 IAxisJob，数据来自代码实现与 AxisJobAttribute";
        });
        Options(options => options.WithTags("定时任务")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(
                PermissionPolicy.AnyOf,
                SystemPermissionCodes.Job.Query,
                SystemPermissionCodes.Job.Create,
                SystemPermissionCodes.Job.Edit));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.ListExecutorsAsync(ct);
}
