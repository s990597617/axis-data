using Axis.Application.System.Config;
using Axis.Application.System.Config.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Config;

/// <summary>
/// 更新参数配置。
/// </summary>
public sealed class UpdateConfigEndpoint(IConfigAppService service) : AxisEndpoint<SaveConfigInput>
{
    public override void Configure()
    {
        Put("/sys/config/{Id}");
        Summary(summary => summary.Summary = "更新参数配置");
        Options(options => options.WithTags("参数配置")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Config.Edit));
    }

    public override async Task HandleAsync(SaveConfigInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
