using Axis.Application.System.Dict;
using Axis.Application.System.Dict.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Dict;

/// <summary>
/// 删除字典数据项。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Dict.DataDelete"/> 权限。
/// </remarks>
public sealed class DeleteDictDataEndpoint(IDictAppService service) : AxisEndpoint<DictDataIdRequest>
{
    public override void Configure()
    {
        Delete("/sys/dict/data/{Id}");
        Summary(summary => summary.Summary = "删除字典数据项");
        Options(options => options.WithTags("字典管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Dict.DataDelete));
    }

    public override async Task HandleAsync(DictDataIdRequest request, CancellationToken ct)
        => await service.DeleteDataAsync(request.Id, ct);
}
