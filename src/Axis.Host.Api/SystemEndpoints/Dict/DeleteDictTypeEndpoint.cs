using Axis.Application.System.Dict;
using Axis.Application.System.Dict.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Dict;

/// <summary>
/// 删除字典类型。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Dict.Delete"/> 权限。
/// </remarks>
public sealed class DeleteDictTypeEndpoint(IDictAppService service) : AxisEndpoint<DictTypeIdRequest>
{
    public override void Configure()
    {
        Delete("/sys/dict/type/{Id}");
        Summary(summary => summary.Summary = "删除字典类型");
        Options(options => options.WithTags("字典管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Dict.Delete));
    }

    public override async Task HandleAsync(DictTypeIdRequest request, CancellationToken ct)
        => await service.DeleteTypeAsync(request.Id, ct);
}
