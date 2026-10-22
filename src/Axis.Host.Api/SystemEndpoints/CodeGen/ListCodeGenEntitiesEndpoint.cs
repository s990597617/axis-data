using Axis.Application.System.CodeGen;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.CodeGen;

/// <summary>
/// 列出 Domain 实体名（供 FromEntity 选择）。
/// </summary>
public sealed class ListCodeGenEntitiesEndpoint(ICodeGenAppService service)
    : AxisEndpointWithoutRequest<IReadOnlyList<string>>
{
    public override void Configure()
    {
        Get("/sys/code-gen/entities");
        Summary(s => s.Summary = "代码生成-实体列表");
        Options(x => x.WithTags("代码生成")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.CodeGen.Query));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.ListEntitiesAsync(ct);
}
