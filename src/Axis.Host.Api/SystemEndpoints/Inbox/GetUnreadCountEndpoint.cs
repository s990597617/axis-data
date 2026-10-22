using Axis.Application.System.Inbox;
using Axis.Application.System.Inbox.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Inbox;

/// <summary>
/// 获取当前用户未读消息数。
/// </summary>
public sealed class GetUnreadCountEndpoint(IInboxAppService service)
    : AxisEndpointWithoutRequest<UnreadCountDto>
{
    public override void Configure()
    {
        Get("/sys/inbox/unread-count");
        Summary(s => s.Summary = "获取未读消息数");
        // 顶栏铃铛全员可用，与菜单「我的消息」查询权限对齐
        Options(o => o.WithTags("收件箱")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Inbox.Query));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.GetUnreadCountAsync(ct);
}
