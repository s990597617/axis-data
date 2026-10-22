using Axis.Application.System.Dict;
using Axis.Application.System.Dict.Dto;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Dict;

/// <summary>
/// 按字典类型编码读取启用数据项（走缓存，供业务下拉与列表翻译）。
/// </summary>
/// <remarks>
/// 仅需登录；不要求字典管理权限。平台字典与当前租户字典由缓存门面按 scope 解析。
/// </remarks>
public sealed class ListDictItemsByCodeEndpoint(IDictAppService service)
    : AxisEndpoint<DictItemsByCodeQuery, IReadOnlyList<DictCacheItemDto>>
{
    public override void Configure()
    {
        Get("/sys/dict/items");
        Summary(summary =>
        {
            summary.Summary = "按编码读取字典项（缓存）";
            summary.Description = "业务侧下拉与 value→label 翻译；读 ISysDictCache，不查管理端分页接口。";
        });
        Options(options => options.WithTags("字典管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule)));
    }

    public override async Task HandleAsync(DictItemsByCodeQuery request, CancellationToken ct)
        => Response = await service.ListItemsByCodeAsync(request.Code, ct);
}
