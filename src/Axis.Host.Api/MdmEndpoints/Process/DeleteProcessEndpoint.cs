using Axis.Application.Mdm.Process;
using Axis.Application.Mdm.Process.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Process;

/// <summary>
/// 删除工序。
/// </summary>
public sealed class DeleteProcessEndpoint(IProcessAppService service) : AxisEndpoint<ProcessIdRequest>
{
    public override void Configure()
    {
        Delete("/mdm/process/{Id}");
        Summary(summary => summary.Summary = "删除工序");
        Options(options => options.WithTags("工序")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.Process.Delete));
    }

    public override async Task HandleAsync(ProcessIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
