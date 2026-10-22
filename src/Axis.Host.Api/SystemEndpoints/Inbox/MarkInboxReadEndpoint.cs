using Axis.Application.System.Inbox;
using Axis.Application.System.Inbox.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Inbox;

/// <summary>
/// 标记收件箱单条已读。
/// </summary>
public sealed class MarkInboxReadEndpoint(IInboxAppService service) : AxisEndpoint<InboxItemRequest>
{
    public override void Configure()
    {
        Put("/sys/inbox/{Id}/read");
        Summary(s => s.Summary = "标记收件箱单条已读");
        Options(o => o.WithTags("收件箱")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Inbox.Read));
    }

    public override async Task HandleAsync(InboxItemRequest request, CancellationToken ct)
        => await service.MarkReadAsync(request.Id, ct);
}
