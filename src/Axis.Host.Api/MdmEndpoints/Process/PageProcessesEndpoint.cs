using Axis.Application.Mdm.Process;
using Axis.Application.Mdm.Process.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Process;

/// <summary>
/// 分页查询工序。
/// </summary>
public sealed class PageProcessesEndpoint(IProcessAppService service)
    : AxisEndpoint<ProcessPageQuery, SqlSugarPagedList<ProcessDto>>
{
    public override void Configure()
    {
        Get("/mdm/process/page");
        Summary(summary => summary.Summary = "工序分页列表");
        Options(options => options.WithTags("工序")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.Process.Query));
    }

    public override async Task HandleAsync(ProcessPageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
