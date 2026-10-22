using Axis.Application.Demo.Sku;
using Axis.Application.Demo.Sku.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.Sku;

/// <summary>
/// 删除SKU。
/// </summary>
public sealed class DeleteSkuEndpoint(ISkuAppService service) : AxisEndpoint<SkuIdRequest>
{
    public override void Configure()
    {
        Delete("/demo/sku/{Id}");
        Summary(summary => summary.Summary = "删除SKU");
        Options(options => options.WithTags("SKU管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.Sku.Delete));
    }

    public override async Task HandleAsync(SkuIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
