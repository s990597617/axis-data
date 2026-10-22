using Axis.Application.System.CodeGen;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.CodeGen;

/// <summary>
/// 列出主库表名（供 FromTable 选择）。
/// </summary>
public sealed class ListCodeGenTablesEndpoint(ICodeGenAppService service)
    : AxisEndpointWithoutRequest<IReadOnlyList<string>>
{
    public override void Configure()
    {
        Get("/sys/code-gen/tables");
        Summary(s => s.Summary = "代码生成-库表列表");
        Options(x => x.WithTags("代码生成")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.CodeGen.Query));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.ListTablesAsync(ct);
}
