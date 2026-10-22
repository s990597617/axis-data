using Axis.Application.System.File;
using Axis.Application.System.File.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.File;

/// <summary>
/// 分页查询文件记录。
/// </summary>
public sealed class PageFilesEndpoint(IFileAppService fileService)
    : AxisEndpoint<FilePageQuery, SqlSugarPagedList<FileDto>>
{
    public override void Configure()
    {
        Get("/sys/file/page");
        Summary(s => s.Summary = "分页查询文件");
        Options(x => x.WithTags("文件管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.File.Query));
    }

    public override async Task HandleAsync(FilePageQuery request, CancellationToken ct)
        => Response = await fileService.PageAsync(request, ct);
}
