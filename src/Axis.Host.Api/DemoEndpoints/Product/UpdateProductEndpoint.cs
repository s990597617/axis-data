using Axis.Application.Demo.Product;
using Axis.Application.Demo.Product.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.Product;

/// <summary>
/// 更新产品。
/// </summary>
public sealed class UpdateProductEndpoint(IProductAppService service) : AxisEndpoint<SaveProductInput>
{
    public override void Configure()
    {
        Put("/demo/product/{Id}");
        Summary(summary => summary.Summary = "更新产品");
        Options(options => options.WithTags("产品管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.Product.Edit));
    }

    public override async Task HandleAsync(SaveProductInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
