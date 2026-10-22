using Axis.Application.System.Inbox;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Inbox;

/// <summary>
/// 收件箱全部标为已读。
/// </summary>
public sealed class MarkInboxAllReadEndpoint(IInboxAppService service) : AxisEndpointWithoutRequest
{
    public override void Configure()
    {
        Put("/sys/inbox/read-all");
        Summary(s => s.Summary = "收件箱全部标为已读");
        Options(o => o.WithTags("收件箱")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Inbox.ReadAll));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => await service.MarkAllReadAsync(ct);
}
