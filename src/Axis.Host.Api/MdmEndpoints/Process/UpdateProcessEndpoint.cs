using Axis.Application.Mdm.Process;
using Axis.Application.Mdm.Process.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Process;

/// <summary>
/// 更新工序。
/// </summary>
public sealed class UpdateProcessEndpoint(IProcessAppService service) : AxisEndpoint<SaveProcessInput>
{
    public override void Configure()
    {
        Put("/mdm/process/{Id}");
        Summary(summary => summary.Summary = "更新工序");
        Options(options => options.WithTags("工序")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.Process.Edit)
            .Transactional());
    }

    public override async Task HandleAsync(SaveProcessInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
