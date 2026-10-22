using Axis.Application.Demo.Brand;
using Axis.Application.Demo.Brand.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.Brand;

/// <summary>
/// 更新品牌。
/// </summary>
public sealed class UpdateBrandEndpoint(IBrandAppService service) : AxisEndpoint<SaveBrandInput>
{
    public override void Configure()
    {
        Put("/demo/brand/{Id}");
        Summary(summary => summary.Summary = "更新品牌");
        Options(options => options.WithTags("品牌管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.Brand.Edit));
    }

    public override async Task HandleAsync(SaveBrandInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
