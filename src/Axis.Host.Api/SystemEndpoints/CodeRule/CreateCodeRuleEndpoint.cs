using Axis.Application.System.CodeRule;
using Axis.Application.System.CodeRule.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.CodeRule;

/// <summary>
/// 新增编码规则。
/// </summary>
public sealed class CreateCodeRuleEndpoint(ICodeRuleAppService service) : AxisEndpoint<SaveCodeRuleInput>
{
    public override void Configure()
    {
        Post("/sys/code-rule");
        Summary(summary => summary.Summary = "新增编码规则");
        Options(options => options.WithTags("编码规则")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.CodeRule.Create));
    }

    public override async Task HandleAsync(SaveCodeRuleInput request, CancellationToken ct)
        => await service.CreateAsync(request, ct);
}
