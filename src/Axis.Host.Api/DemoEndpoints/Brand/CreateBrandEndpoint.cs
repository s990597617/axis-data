using Axis.Application.Demo.Brand;
using Axis.Application.Demo.Brand.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.Brand;

/// <summary>
/// 新增品牌。
/// </summary>
public sealed class CreateBrandEndpoint(IBrandAppService service) : AxisEndpoint<SaveBrandInput>
{
    public override void Configure()
    {
        Post("/demo/brand");
        Summary(summary => summary.Summary = "新增品牌");
        Options(options => options.WithTags("品牌管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.Brand.Create));
    }

    public override async Task HandleAsync(SaveBrandInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
