using Axis.Application.System.Notice;
using Axis.Application.System.Notice.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Notice;

/// <summary>
/// 新增通知公告。
/// </summary>
public sealed class CreateNoticeEndpoint(INoticeAppService service) : AxisEndpoint<SaveNoticeInput>
{
    public override void Configure()
    {
        Post("/sys/notice");
        Summary(s => s.Summary = "新增通知公告");
        Options(o => o.WithTags("通知公告")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Notice.Create));
    }

    public override async Task HandleAsync(SaveNoticeInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
