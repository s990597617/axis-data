using Axis.Application.Demo.Brand;
using Axis.Application.Demo.Brand.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.Brand;

/// <summary>
/// 分页查询品牌。
/// </summary>
public sealed class PageBrandsEndpoint(IBrandAppService service)
    : AxisEndpoint<BrandPageQuery, SqlSugarPagedList<BrandDto>>
{
    public override void Configure()
    {
        Get("/demo/brand/page");
        Summary(summary => summary.Summary = "品牌分页列表");
        Options(options => options.WithTags("品牌管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.Brand.Query));
    }

    public override async Task HandleAsync(BrandPageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
