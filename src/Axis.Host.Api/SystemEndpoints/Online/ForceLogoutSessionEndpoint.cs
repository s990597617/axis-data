using Axis.Application.System.Online;
using Axis.Application.System.Online.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Online;

/// <summary>
/// 强制下线指定会话。
/// </summary>
public sealed class ForceLogoutSessionEndpoint(IOnlineSessionAppService service)
    : AxisEndpoint<KickOnlineSessionRequest>
{
    public override void Configure()
    {
        Post("/sys/online/{SessionId}/kick");
        Summary(s => s.Summary = "强制下线指定会话");
        Options(x => x.WithTags("在线用户")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Online.Kick));
    }

    public override async Task HandleAsync(KickOnlineSessionRequest req, CancellationToken ct)
        => await service.ForceLogoutSessionAsync(req.SessionId, ct: ct);
}
