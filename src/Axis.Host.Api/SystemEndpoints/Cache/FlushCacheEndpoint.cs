using Axis.Application.System.Cache;
using Axis.Application.System.Cache.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Cache;

/// <summary>
/// 清空全部缓存（非 Production）。
/// </summary>
public sealed class FlushCacheEndpoint(ICacheAdminAppService service)
    : AxisEndpointWithoutRequest<CacheClearResultDto>
{
    public override void Configure()
    {
        Delete("/sys/cache/flush");
        Summary(s => s.Summary = "清空全部缓存");
        Options(x => x.WithTags("缓存管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Cache.Clear));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.FlushAllAsync(ct);
}
