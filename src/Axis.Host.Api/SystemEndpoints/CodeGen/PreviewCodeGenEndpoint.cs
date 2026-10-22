using Axis.Application.System.CodeGen;
using Axis.Application.System.CodeGen.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.CodeGen;

/// <summary>
/// 预览模块生成（dry-run，不落盘）。
/// </summary>
public sealed class PreviewCodeGenEndpoint(ICodeGenAppService service)
    : AxisEndpoint<GenerateModuleInput, CodeGenResultDto>
{
    public override void Configure()
    {
        Post("/sys/code-gen/preview");
        Summary(s => s.Summary = "预览模块代码生成");
        Options(x => x.WithTags("代码生成")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.CodeGen.Preview));
    }

    public override async Task HandleAsync(GenerateModuleInput request, CancellationToken ct)
        => Response = await service.PreviewAsync(request, ct);
}
