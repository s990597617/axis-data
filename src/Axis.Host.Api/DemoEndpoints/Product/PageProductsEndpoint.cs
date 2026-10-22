using Axis.Application.Demo.Product;
using Axis.Application.Demo.Product.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.Product;

/// <summary>
/// 分页查询产品。
/// </summary>
public sealed class PageProductsEndpoint(IProductAppService service)
    : AxisEndpoint<ProductPageQuery, SqlSugarPagedList<ProductDto>>
{
    public override void Configure()
    {
        Get("/demo/product/page");
        Summary(summary => summary.Summary = "产品分页列表");
        Options(options => options.WithTags("产品管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.Product.Query));
    }

    public override async Task HandleAsync(ProductPageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
