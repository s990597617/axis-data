using Axis.Application.System.Dict;
using Axis.Application.System.Dict.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Dict;

/// <summary>
/// 分页查询字典类型列表。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Dict.Query"/> 权限。
/// </remarks>
public sealed class PageDictTypesEndpoint(IDictAppService service)
    : AxisEndpoint<DictTypePageQuery, SqlSugarPagedList<DictTypeDto>>
{
    public override void Configure()
    {
        Get("/sys/dict/type/page");
        Summary(summary => summary.Summary = "字典类型分页列表");
        Options(options => options.WithTags("字典管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Dict.Query));
    }

    public override async Task HandleAsync(DictTypePageQuery request, CancellationToken ct)
        => Response = await service.PageTypesAsync(request, ct);
}
