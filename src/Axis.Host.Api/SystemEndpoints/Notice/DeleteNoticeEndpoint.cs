using Axis.Application.System.Notice;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Notice;

/// <summary>
/// 删除通知公告。
/// </summary>
public sealed class DeleteNoticeEndpoint(INoticeAppService service) : AxisEndpoint<DeleteNoticeRequest>
{
    public override void Configure()
    {
        Delete("/sys/notice/{Id}");
        Summary(s => s.Summary = "删除通知公告");
        Options(o => o.WithTags("通知公告")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Notice.Delete));
    }

    public override async Task HandleAsync(DeleteNoticeRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
