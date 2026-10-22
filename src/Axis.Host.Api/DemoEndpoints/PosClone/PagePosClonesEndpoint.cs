using Axis.Application.Demo.PosClone;
using Axis.Application.Demo.PosClone.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.PosClone;

/// <summary>
/// 分页查询岗位副本。
/// </summary>
public sealed class PagePosClonesEndpoint(IPosCloneAppService service)
    : AxisEndpoint<PosClonePageQuery, SqlSugarPagedList<PosCloneDto>>
{
    public override void Configure()
    {
        Get("/demo/pos-clone/page");
        Summary(summary => summary.Summary = "岗位副本分页列表");
        Options(options => options.WithTags("岗位副本管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.PosClone.Query));
    }

    public override async Task HandleAsync(PosClonePageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
