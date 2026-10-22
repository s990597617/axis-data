using Axis.Application.System.Dict;
using Axis.Application.System.Dict.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Dict;

/// <summary>
/// 查询指定字典类型的数据项列表。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Dict.DataQuery"/> 权限。
/// </remarks>
public sealed class ListDictDataEndpoint(IDictAppService service)
    : AxisEndpoint<DictDataListQuery, IReadOnlyList<DictDataDto>>
{
    public override void Configure()
    {
        Get("/sys/dict/data/list");
        Summary(summary => summary.Summary = "字典数据项列表");
        Options(options => options.WithTags("字典管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Dict.DataQuery));
    }

    public override async Task HandleAsync(DictDataListQuery request, CancellationToken ct)
        => Response = await service.ListDataAsync(request, ct);
}
