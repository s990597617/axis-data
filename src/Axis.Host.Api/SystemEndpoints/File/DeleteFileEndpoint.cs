using Axis.Application.System.File;
using Axis.Application.System.File.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.File;

/// <summary>
/// 删除指定文件（物理文件 + 库记录标记删除）。
/// </summary>
public sealed class DeleteFileEndpoint(IFileAppService fileService)
    : AxisEndpoint<FileIdRequest>
{
    public override void Configure()
    {
        Delete("/sys/file/{FileId}");
        Summary(s => s.Summary = "删除文件");
        Options(x => x.WithTags("文件管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.File.Delete));
    }

    public override async Task HandleAsync(FileIdRequest req, CancellationToken ct)
        => await fileService.DeleteAsync(req.FileId, ct);
}
