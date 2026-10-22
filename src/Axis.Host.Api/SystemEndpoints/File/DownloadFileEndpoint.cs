using System.Net.Mime;
using System.Net.Http.Headers;
using Axis.Application.System.File;
using Axis.Application.System.File.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.File;

/// <summary>
/// 按 FileId 下载文件流。
/// </summary>
public sealed class DownloadFileEndpoint(IFileAppService fileService)
    : AxisEndpoint<FileIdRequest>
{
    public override void Configure()
    {
        Get("/sys/file/{FileId}/download");
        Summary(s => s.Summary = "下载文件");
        Options(x => x.WithTags("文件管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.File.Download));
    }

    public override async Task HandleAsync(FileIdRequest req, CancellationToken ct)
    {
        var file = await fileService.OpenDownloadAsync(req.FileId, ct);
        await using (file.Stream)
        {
            var disposition = new ContentDispositionHeaderValue(DispositionTypeNames.Attachment)
            {
                FileNameStar = file.FileName
            };
            HttpContext.Response.ContentType = file.ContentType;
            HttpContext.Response.Headers.ContentDisposition = disposition.ToString();
            await file.Stream.CopyToAsync(HttpContext.Response.Body, ct);
        }
    }
}
