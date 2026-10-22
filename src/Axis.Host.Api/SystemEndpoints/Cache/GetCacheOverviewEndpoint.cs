using Axis.Application.System.Cache;
using Axis.Application.System.Cache.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Cache;

/// <summary>
/// 获取缓存运行概览。
/// </summary>
public sealed class GetCacheOverviewEndpoint(ICacheAdminAppService service)
    : AxisEndpointWithoutRequest<CacheOverviewDto>
{
    public override void Configure()
    {
        Get("/sys/cache/overview");
        Summary(s => s.Summary = "缓存运行概览");
        Options(x => x.WithTags("缓存管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Cache.Query));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.GetOverviewAsync(ct);
}
