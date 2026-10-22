using Axis.Application.Demo.Sku;
using Axis.Application.Demo.Sku.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.Sku;

/// <summary>
/// 更新SKU。
/// </summary>
public sealed class UpdateSkuEndpoint(ISkuAppService service) : AxisEndpoint<SaveSkuInput>
{
    public override void Configure()
    {
        Put("/demo/sku/{Id}");
        Summary(summary => summary.Summary = "更新SKU");
        Options(options => options.WithTags("SKU管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.Sku.Edit));
    }

    public override async Task HandleAsync(SaveSkuInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
