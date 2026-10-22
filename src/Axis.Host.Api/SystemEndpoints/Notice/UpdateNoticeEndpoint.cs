using Axis.Application.System.Notice;
using Axis.Application.System.Notice.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Notice;

/// <summary>
/// 更新通知公告。
/// </summary>
public sealed class UpdateNoticeEndpoint(INoticeAppService service) : AxisEndpoint<SaveNoticeInput>
{
    public override void Configure()
    {
        Put("/sys/notice/{Id}");
        Summary(s => s.Summary = "更新通知公告");
        Options(o => o.WithTags("通知公告")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Notice.Edit));
    }

    public override async Task HandleAsync(SaveNoticeInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
