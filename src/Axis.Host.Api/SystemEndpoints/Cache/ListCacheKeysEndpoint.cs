using Axis.Application.System.Cache;
using Axis.Application.System.Cache.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Cache;

/// <summary>
/// 按前缀列出缓存键。
/// </summary>
public sealed class ListCacheKeysEndpoint(ICacheAdminAppService service)
    : AxisEndpoint<CacheKeyQuery, CacheKeyListDto>
{
    public override void Configure()
    {
        Get("/sys/cache/keys");
        Summary(s => s.Summary = "按前缀查询缓存键");
        Options(x => x.WithTags("缓存管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Cache.Query));
    }

    public override async Task HandleAsync(CacheKeyQuery request, CancellationToken ct)
        => Response = await service.ListKeysAsync(request, ct);
}
