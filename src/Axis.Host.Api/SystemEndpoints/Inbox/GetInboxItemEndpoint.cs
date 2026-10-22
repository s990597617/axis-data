using Axis.Application.System.Inbox;
using Axis.Application.System.Inbox.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;
using Axis.Infrastructure.Exceptions;

namespace Axis.Host.Api.SystemEndpoints.Inbox;

/// <summary>
/// 获取收件箱单条详情。
/// </summary>
public sealed class GetInboxItemEndpoint(IInboxAppService service)
    : AxisEndpoint<InboxItemRequest, InboxItemDto>
{
    public override void Configure()
    {
        Get("/sys/inbox/{Id}");
        Summary(s => s.Summary = "获取收件箱详情");
        Options(o => o.WithTags("收件箱")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Inbox.Query));
    }

    public override async Task HandleAsync(InboxItemRequest request, CancellationToken ct)
        => Response = await service.GetAsync(request.Id, ct)
            ?? throw new BusinessException("记录不存在");
}
