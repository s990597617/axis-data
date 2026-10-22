using Axis.Application.System.Dict;
using Axis.Application.System.Dict.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Dict;

/// <summary>
/// 新增字典类型。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Dict.Create"/> 权限。
/// </remarks>
public sealed class CreateDictTypeEndpoint(IDictAppService service) : AxisEndpoint<SaveDictTypeInput>
{
    public override void Configure()
    {
        Post("/sys/dict/type");
        Summary(summary => summary.Summary = "新增字典类型");
        Options(options => options.WithTags("字典管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Dict.Create));
    }

    public override async Task HandleAsync(SaveDictTypeInput request, CancellationToken ct)
        => await service.CreateTypeAsync(request, ct);
}
