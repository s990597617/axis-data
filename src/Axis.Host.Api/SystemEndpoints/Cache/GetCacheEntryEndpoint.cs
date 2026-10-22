using Axis.Application.System.Cache;
using Axis.Application.System.Cache.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Cache;

/// <summary>
/// 查看单条缓存详情。
/// </summary>
public sealed class GetCacheEntryEndpoint(ICacheAdminAppService service)
    : AxisEndpoint<CacheEntryQuery, CacheEntryDto>
{
    public override void Configure()
    {
        Get("/sys/cache/entry");
        Summary(s => s.Summary = "查看缓存条目");
        Options(x => x.WithTags("缓存管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Cache.Query));
    }

    public override async Task HandleAsync(CacheEntryQuery request, CancellationToken ct)
        => Response = await service.GetEntryAsync(request, ct);
}
