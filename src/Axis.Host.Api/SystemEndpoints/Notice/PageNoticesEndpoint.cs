using Axis.Application.System.Notice;
using Axis.Application.System.Notice.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Notice;

/// <summary>
/// 通知公告分页列表。
/// </summary>
public sealed class PageNoticesEndpoint(INoticeAppService service)
    : AxisEndpoint<NoticePageQuery, SqlSugarPagedList<NoticeDto>>
{
    public override void Configure()
    {
        Get("/sys/notice/page");
        Summary(s => s.Summary = "通知公告分页列表");
        Options(o => o.WithTags("通知公告")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Notice.Query));
    }

    public override async Task HandleAsync(NoticePageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
