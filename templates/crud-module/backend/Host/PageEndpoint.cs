using Axis.Application.__Area__.__Module__;
using Axis.Application.__Area__.__Module__.Dto;
using Axis.Domain.__Area__;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.__Area__Endpoints.__Module__;

/// <summary>
/// 分页查询__CnName__。
/// </summary>
public sealed class Page__Module__sEndpoint(I__Module__AppService service)
    : AxisEndpoint<__Module__PageQuery, SqlSugarPagedList<__Module__Dto>>
{
    public override void Configure()
    {
        Get("__RoutePrefix__/page");
        Summary(summary => summary.Summary = "__CnName__分页列表");
        Options(options => options.WithTags("__CnName__管理")
            .WithGroupName(nameof(ApiGroupEnum.__ApiGroup__))
            .RequirePermission(__PermissionCodesType__.__Module__.Query));
    }

    public override async Task HandleAsync(__Module__PageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
