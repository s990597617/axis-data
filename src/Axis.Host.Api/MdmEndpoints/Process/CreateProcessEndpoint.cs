using Axis.Application.Mdm.Process;
using Axis.Application.Mdm.Process.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Process;

/// <summary>
/// 新增工序。
/// </summary>
public sealed class CreateProcessEndpoint(IProcessAppService service) : AxisEndpoint<SaveProcessInput>
{
    public override void Configure()
    {
        Post("/mdm/process");
        Summary(summary => summary.Summary = "新增工序");
        Options(options => options.WithTags("工序")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.Process.Create)
            .Transactional());
    }

    public override async Task HandleAsync(SaveProcessInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
