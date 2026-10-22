using Axis.Application.System.File;
using Axis.Application.System.File.Dto;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.File;

/// <summary>
/// 通用文件上传。
/// </summary>
public sealed class UploadFileEndpoint(IFileAppService fileService)
    : AxisEndpoint<UploadFileRequest, UploadFileResultDto>
{
    public override void Configure()
    {
        Post("/sys/file/upload");
        AllowFileUploads();
        Summary(s => s.Summary = "上传文件");
        Options(x => x.WithTags("文件管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule)));
    }

    public override async Task HandleAsync(UploadFileRequest req, CancellationToken ct)
    {
        EnsureTrue(req.File is { Length: > 0 }, "请选择要上传的文件");

        await using var stream = req.File.OpenReadStream();
        Response = await fileService.UploadAsync(
            new UploadFileCommand
            {
                Stream = stream,
                FileName = req.File.FileName,
                ContentType = req.File.ContentType ?? "application/octet-stream",
                Size = req.File.Length,
                Module = req.Module
            },
            ct);
    }
}
