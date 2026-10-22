using Axis.Application.Demo.PosClone;
using Axis.Application.Demo.PosClone.Dto;
using Axis.Domain.Demo;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.DemoEndpoints.PosClone;

/// <summary>
/// 新增岗位副本。
/// </summary>
public sealed class CreatePosCloneEndpoint(IPosCloneAppService service) : AxisEndpoint<SavePosCloneInput>
{
    public override void Configure()
    {
        Post("/demo/pos-clone");
        Summary(summary => summary.Summary = "新增岗位副本");
        Options(options => options.WithTags("岗位副本管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminBaseModule))
            .RequirePermission(DemoPermissionCodes.PosClone.Create));
    }

    public override async Task HandleAsync(SavePosCloneInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
