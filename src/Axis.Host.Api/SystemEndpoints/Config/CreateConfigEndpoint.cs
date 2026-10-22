using Axis.Application.System.Config;
using Axis.Application.System.Config.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Config;

/// <summary>
/// 新增参数配置。
/// </summary>
public sealed class CreateConfigEndpoint(IConfigAppService service) : AxisEndpoint<SaveConfigInput>
{
    public override void Configure()
    {
        Post("/sys/config");
        Summary(summary => summary.Summary = "新增参数配置");
        Options(options => options.WithTags("参数配置")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Config.Create));
    }

    public override async Task HandleAsync(SaveConfigInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
