using Axis.Application.Demo.Brand;
using Axis.Application.Demo.Brand.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.Brand;

/// <summary>
/// 删除品牌。
/// </summary>
public sealed class DeleteBrandEndpoint(IBrandAppService service) : AxisEndpoint<BrandIdRequest>
{
    public override void Configure()
    {
        Delete("/demo/brand/{Id}");
        Summary(summary => summary.Summary = "删除品牌");
        Options(options => options.WithTags("品牌管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.Brand.Delete));
    }

    public override async Task HandleAsync(BrandIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
