using Axis.Application.Common;
using Axis.Application.System.User;
using Axis.Application.System.User.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.User;

/// <summary>
/// 批量导入用户（xlsx）。
/// </summary>
public sealed class ImportUsersEndpoint(IUserAppService userService)
    : AxisEndpoint<ImportUsersRequest, ImportResultDto>
{
    public override void Configure()
    {
        Post("/sys/user/import");
        AllowFileUploads();
        Summary(s => s.Summary = "导入用户");
        Options(x => x.WithTags("用户管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.User.Import)
            .Transactional());
    }

    public override async Task HandleAsync(ImportUsersRequest req, CancellationToken ct)
    {
        EnsureTrue(req.File is { Length: > 0 }, "请选择要导入的 Excel 文件");
        await using var stream = req.File!.OpenReadStream();
        Response = await userService.ImportAsync(stream, ct);
    }
}
