using Axis.Application.System.Dict;
using Axis.Application.System.Dict.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Dict;

/// <summary>
/// 更新字典类型主表字段。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Dict.Edit"/> 权限；路由绑定 <c>Id</c>。
/// </remarks>
public sealed class UpdateDictTypeEndpoint(IDictAppService service) : AxisEndpoint<SaveDictTypeInput>
{
    public override void Configure()
    {
        Put("/sys/dict/type/{Id}");
        Summary(summary => summary.Summary = "更新字典类型");
        Options(options => options.WithTags("字典管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Dict.Edit));
    }

    public override async Task HandleAsync(SaveDictTypeInput request, CancellationToken ct)
        => await service.UpdateTypeAsync(request.Id, request, ct);
}
