using Axis.Application.Demo.PosClone;
using Axis.Application.Demo.PosClone.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.PosClone;

/// <summary>
/// 更新岗位副本。
/// </summary>
public sealed class UpdatePosCloneEndpoint(IPosCloneAppService service) : AxisEndpoint<SavePosCloneInput>
{
    public override void Configure()
    {
        Put("/demo/pos-clone/{Id}");
        Summary(summary => summary.Summary = "更新岗位副本");
        Options(options => options.WithTags("岗位副本管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.PosClone.Edit));
    }

    public override async Task HandleAsync(SavePosCloneInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
