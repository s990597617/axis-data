using Axis.Application.Demo.Product;
using Axis.Application.Demo.Product.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.Product;

/// <summary>
/// 删除产品。
/// </summary>
public sealed class DeleteProductEndpoint(IProductAppService service) : AxisEndpoint<ProductIdRequest>
{
    public override void Configure()
    {
        Delete("/demo/product/{Id}");
        Summary(summary => summary.Summary = "删除产品");
        Options(options => options.WithTags("产品管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.Product.Delete));
    }

    public override async Task HandleAsync(ProductIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
