using Axis.Application.System.Online;
using Axis.Application.System.Online.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Online;

/// <summary>
/// 强制下线用户全部会话。
/// </summary>
public sealed class ForceLogoutUserEndpoint(IOnlineSessionAppService service)
    : AxisEndpoint<KickOnlineUserRequest>
{
    public override void Configure()
    {
        Post("/sys/online/user/{UserId}/kick");
        Summary(s => s.Summary = "强制下线用户全部会话");
        Options(x => x.WithTags("在线用户")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Online.Kick));
    }

    public override async Task HandleAsync(KickOnlineUserRequest req, CancellationToken ct)
        => await service.ForceLogoutUserAsync(req.UserId, ct: ct);
}
