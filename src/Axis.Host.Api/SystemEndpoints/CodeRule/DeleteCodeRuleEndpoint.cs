using Axis.Application.System.CodeRule;
using Axis.Application.System.CodeRule.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.CodeRule;

/// <summary>
/// 删除编码规则。
/// </summary>
public sealed class DeleteCodeRuleEndpoint(ICodeRuleAppService service) : AxisEndpoint<CodeRuleIdRequest>
{
    public override void Configure()
    {
        Delete("/sys/code-rule/{Id}");
        Summary(summary => summary.Summary = "删除编码规则");
        Options(options => options.WithTags("编码规则")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.CodeRule.Delete));
    }

    public override async Task HandleAsync(CodeRuleIdRequest request, CancellationToken ct)
        => await service.DeleteAsync(request.Id, ct);
}
