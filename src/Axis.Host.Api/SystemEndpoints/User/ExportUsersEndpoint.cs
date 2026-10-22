using System.Net.Http.Headers;
using System.Net.Mime;
using Axis.Application.System.User;
using Axis.Application.System.User.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.User;

/// <summary>
/// 按筛选条件导出用户 Excel。
/// </summary>
public sealed class ExportUsersEndpoint(IUserAppService userService)
    : AxisEndpoint<UserPageQuery>
{
    public override void Configure()
    {
        Get("/sys/user/export");
        Summary(s => s.Summary = "导出用户");
        Options(x => x.WithTags("用户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.User.Export));
    }

    public override async Task HandleAsync(UserPageQuery req, CancellationToken ct)
    {
        var file = await userService.ExportAsync(req, ct);
        var disposition = new ContentDispositionHeaderValue(DispositionTypeNames.Attachment)
        {
            FileNameStar = file.FileName
        };
        HttpContext.Response.ContentType = file.ContentType;
        HttpContext.Response.Headers.ContentDisposition = disposition.ToString();
        await HttpContext.Response.Body.WriteAsync(file.Content, ct);
    }
}
