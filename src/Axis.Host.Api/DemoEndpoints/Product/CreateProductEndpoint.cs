using Axis.Application.Demo.Product;
using Axis.Application.Demo.Product.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.Product;

/// <summary>
/// 新增产品。
/// </summary>
public sealed class CreateProductEndpoint(IProductAppService service) : AxisEndpoint<SaveProductInput>
{
    public override void Configure()
    {
        Post("/demo/product");
        Summary(summary => summary.Summary = "新增产品");
        Options(options => options.WithTags("产品管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.Product.Create));
    }

    public override async Task HandleAsync(SaveProductInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
