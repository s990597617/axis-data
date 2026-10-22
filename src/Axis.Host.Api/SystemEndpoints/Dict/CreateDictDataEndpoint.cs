using Axis.Application.System.Dict;
using Axis.Application.System.Dict.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.Dict;

/// <summary>
/// 新增字典数据项。
/// </summary>
/// <remarks>
/// 需要 <see cref="SystemPermissionCodes.Dict.DataCreate"/> 权限；在事务中执行。
/// </remarks>
public sealed class CreateDictDataEndpoint(IDictAppService service) : AxisEndpoint<SaveDictDataInput>
{
    public override void Configure()
    {
        Post("/sys/dict/data");
        Summary(summary => summary.Summary = "新增字典数据项");
        Options(options => options.WithTags("字典管理")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.Dict.DataCreate)
            .Transactional());
    }

    public override async Task HandleAsync(SaveDictDataInput request, CancellationToken ct)
        => await service.CreateDataAsync(request, ct);
}
