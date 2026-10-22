using Axis.Application.System.Config;
using Axis.Application.System.Config.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Config;

/// <summary>
/// 分页查询参数配置。
/// </summary>
public sealed class PageConfigsEndpoint(IConfigAppService service)
    : AxisEndpoint<ConfigPageQuery, SqlSugarPagedList<ConfigDto>>
{
    public override void Configure()
    {
        Get("/sys/config/page");
        Summary(summary => summary.Summary = "参数配置分页列表");
        Options(options => options.WithTags("参数配置")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Config.Query));
    }

    public override async Task HandleAsync(ConfigPageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
