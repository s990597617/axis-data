using Axis.Application.Mdm.Process;
using Axis.Application.Mdm.Process.Dto;
using Axis.Domain.Mdm;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.MdmEndpoints.Process;

/// <summary>
/// 预览工序下一编码（不占用流水）。
/// </summary>
public sealed class PreviewProcessNextCodeEndpoint(IProcessAppService service)
    : AxisEndpointWithoutRequest<ProcessNextCodeDto>
{
    public override void Configure()
    {
        Get("/mdm/process/next-code");
        Summary(summary => summary.Summary = "预览工序下一编码");
        Options(options => options.WithTags("工序")
            .WithGroupName(nameof(ApiGroupEnum.MesModule))
            .RequirePermission(MdmPermissionCodes.Process.Create));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.PreviewNextCodeAsync(ct);
}
