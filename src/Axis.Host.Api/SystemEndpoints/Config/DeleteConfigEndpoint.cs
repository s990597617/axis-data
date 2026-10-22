using Axis.Application.System.Config;
using Axis.Application.System.Config.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Config;

/// <summary>
/// 删除参数配置。
/// </summary>
public sealed class DeleteConfigEndpoint(IConfigAppService service) : AxisEndpoint<ConfigIdRequest>
{
    public override void Configure()
    {
        Delete("/sys/config/{Id}");
        Summary(summary => summary.Summary = "删除参数配置");
        Options(options => options.WithTags("参数配置")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Config.Delete));
    }

    public override async Task HandleAsync(ConfigIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
