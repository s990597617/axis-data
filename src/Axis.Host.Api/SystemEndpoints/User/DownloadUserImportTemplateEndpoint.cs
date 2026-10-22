using System.Net.Http.Headers;
using System.Net.Mime;
using Axis.Application.System.User;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.User;

/// <summary>
/// 下载用户导入 Excel 模板。
/// </summary>
public sealed class DownloadUserImportTemplateEndpoint(IUserAppService userService)
    : AxisEndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/sys/user/import-template");
        Summary(s => s.Summary = "下载用户导入模板");
        Options(x => x.WithTags("用户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.User.Import));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var file = await userService.GetImportTemplateAsync(ct);
        var disposition = new ContentDispositionHeaderValue(DispositionTypeNames.Attachment)
        {
            FileNameStar = file.FileName
        };
        HttpContext.Response.ContentType = file.ContentType;
        HttpContext.Response.Headers.ContentDisposition = disposition.ToString();
        await HttpContext.Response.Body.WriteAsync(file.Content, ct);
    }
}
