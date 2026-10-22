using Axis.Application.System.Notice;
using Axis.Application.System.Notice.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Notice;

/// <summary>
/// 发布或下架通知公告。
/// </summary>
public sealed class PublishNoticeEndpoint(INoticeAppService service) : AxisEndpoint<PublishNoticeInput>
{
    public override void Configure()
    {
        Put("/sys/notice/{Id}/publish");
        Summary(s => s.Summary = "发布或下架通知公告");
        Options(o => o.WithTags("通知公告")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Notice.Publish));
    }

    public override async Task HandleAsync(PublishNoticeInput request, CancellationToken ct)
        => await service.PublishAsync(request.Id, request.Published, ct);
}
