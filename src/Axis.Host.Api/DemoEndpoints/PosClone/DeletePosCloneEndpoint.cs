using Axis.Application.Demo.PosClone;
using Axis.Application.Demo.PosClone.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.PosClone;

/// <summary>
/// 删除岗位副本。
/// </summary>
public sealed class DeletePosCloneEndpoint(IPosCloneAppService service) : AxisEndpoint<PosCloneIdRequest>
{
    public override void Configure()
    {
        Delete("/demo/pos-clone/{Id}");
        Summary(summary => summary.Summary = "删除岗位副本");
        Options(options => options.WithTags("岗位副本管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.PosClone.Delete));
    }

    public override async Task HandleAsync(PosCloneIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
