using Axis.Application.System.File;
using Axis.Application.System.File.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.File;

/// <summary>
/// 按业务模块统计文件数量。
/// </summary>
public sealed class ListFileModulesEndpoint(IFileAppService fileService)
    : AxisEndpointWithoutRequest<List<FileModuleStatDto>>
{
    public override void Configure()
    {
        Get("/sys/file/modules");
        Summary(s => s.Summary = "文件模块分类统计");
        Options(x => x.WithTags("文件管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.File.Query));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await fileService.ListModulesAsync(ct);
}
