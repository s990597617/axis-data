using Axis.Application.System.Cache;
using Axis.Application.System.Cache.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Cache;

/// <summary>
/// 删除指定缓存键。
/// </summary>
public sealed class RemoveCacheKeyEndpoint(ICacheAdminAppService service)
    : AxisEndpoint<CacheKeyRequest>
{
    public override void Configure()
    {
        Delete("/sys/cache/key");
        Summary(s => s.Summary = "删除指定缓存键");
        Options(x => x.WithTags("缓存管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Cache.Clear));
    }

    public override async Task HandleAsync(CacheKeyRequest request, CancellationToken ct)
        => await service.RemoveKeyAsync(request, ct);
}
