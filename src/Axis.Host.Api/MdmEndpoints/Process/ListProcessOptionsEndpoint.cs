using Axis.Application.Mdm.Process;
using Axis.Application.Mdm.Process.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Attributes;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Process;

/// <summary>
/// 查询启用中的工序，供工艺路线等下拉使用。
/// </summary>
/// <remarks>
/// 归属工序模块；工艺路线维护页选工序也可调用，故允许工序查询或路线查询/新增/编辑权限之一。
/// </remarks>
public sealed class ListProcessOptionsEndpoint(IProcessAppService service)
    : AxisEndpointWithoutRequest<IReadOnlyList<ProcessOptionDto>>
{
    public override void Configure()
    {
        Get("/mdm/process/options");
        Summary(summary => summary.Summary = "启用工序下拉");
        Options(options => options.WithTags("工序")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(
                PermissionPolicy.AnyOf,
                MdmPermissionCodes.Process.Query,
                MdmPermissionCodes.Routing.Query,
                MdmPermissionCodes.Routing.Create,
                MdmPermissionCodes.Routing.Edit));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.ListEnabledAsync(ct);
}
