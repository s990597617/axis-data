using Axis.Application.System.CodeGen;
using Axis.Application.System.CodeGen.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.CodeGen;

/// <summary>
/// 查询代码生成开关与仓库根。
/// </summary>
public sealed class GetCodeGenStatusEndpoint(ICodeGenAppService service)
    : AxisEndpointWithoutRequest<CodeGenStatusDto>
{
    public override void Configure()
    {
        Get("/sys/code-gen/status");
        Summary(s => s.Summary = "代码生成状态");
        Options(x => x.WithTags("代码生成")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.CodeGen.Query));
    }

    public override async Task HandleAsync(CancellationToken ct)
        => Response = await service.GetStatusAsync(ct);
}
