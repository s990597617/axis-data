using Axis.Application.System.Inbox;
using Axis.Application.System.Inbox.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Inbox;

/// <summary>
/// 收件箱分页列表。
/// </summary>
public sealed class PageInboxEndpoint(IInboxAppService service)
    : AxisEndpoint<InboxPageQuery, SqlSugarPagedList<InboxItemDto>>
{
    public override void Configure()
    {
        Get("/sys/inbox/page");
        Summary(s => s.Summary = "收件箱分页列表");
        Options(o => o.WithTags("收件箱")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Inbox.Query));
    }

    public override async Task HandleAsync(InboxPageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
