using Axis.Application.Demo.Sku;
using Axis.Application.Demo.Sku.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.Sku;

/// <summary>
/// 新增SKU。
/// </summary>
public sealed class CreateSkuEndpoint(ISkuAppService service) : AxisEndpoint<SaveSkuInput>
{
    public override void Configure()
    {
        Post("/demo/sku");
        Summary(summary => summary.Summary = "新增SKU");
        Options(options => options.WithTags("SKU管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.Sku.Create));
    }

    public override async Task HandleAsync(SaveSkuInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
