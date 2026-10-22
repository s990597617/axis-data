using Axis.Application.System.Cache;
using Axis.Application.System.Cache.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Cache;

/// <summary>
/// 按前缀清理缓存。
/// </summary>
public sealed class RemoveCacheByPrefixEndpoint(ICacheAdminAppService service)
    : AxisEndpoint<CachePrefixRequest, CacheClearResultDto>
{
    public override void Configure()
    {
        Delete("/sys/cache/prefix");
        Summary(s => s.Summary = "按前缀清理缓存");
        Options(x => x.WithTags("缓存管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Cache.Clear));
    }

    public override async Task HandleAsync(CachePrefixRequest request, CancellationToken ct)
        => Response = await service.RemoveByPrefixAsync(request, ct);
}
