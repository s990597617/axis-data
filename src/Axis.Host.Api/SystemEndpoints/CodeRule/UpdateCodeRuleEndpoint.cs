using Axis.Application.System.CodeRule;
using Axis.Application.System.CodeRule.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.CodeRule;

/// <summary>
/// 更新编码规则。
/// </summary>
public sealed class UpdateCodeRuleEndpoint(ICodeRuleAppService service) : AxisEndpoint<SaveCodeRuleInput>
{
    public override void Configure()
    {
        Put("/sys/code-rule/{Id}");
        Summary(summary => summary.Summary = "更新编码规则");
        Options(options => options.WithTags("编码规则")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.CodeRule.Edit));
    }

    public override async Task HandleAsync(SaveCodeRuleInput request, CancellationToken ct)
        => await service.UpdateAsync(request.Id, request, ct);
}
