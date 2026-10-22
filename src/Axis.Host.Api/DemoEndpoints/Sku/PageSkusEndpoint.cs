using Axis.Application.Demo.Sku;
using Axis.Application.Demo.Sku.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.Sku;

/// <summary>
/// 分页查询SKU。
/// </summary>
public sealed class PageSkusEndpoint(ISkuAppService service)
    : AxisEndpoint<SkuPageQuery, SqlSugarPagedList<SkuDto>>
{
    public override void Configure()
    {
        Get("/demo/sku/page");
        Summary(summary => summary.Summary = "SKU分页列表");
        Options(options => options.WithTags("SKU管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.Sku.Query));
    }

    public override async Task HandleAsync(SkuPageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
