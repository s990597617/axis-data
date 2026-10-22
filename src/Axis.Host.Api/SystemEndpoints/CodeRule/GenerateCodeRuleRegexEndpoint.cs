using Axis.Application.System.CodeRule;
using Axis.Application.System.CodeRule.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.CodeRule;

/// <summary>
/// 按模板生成反向正则。
/// </summary>
public sealed class GenerateCodeRuleRegexEndpoint(ICodeRuleAppService service)
    : AxisEndpoint<SaveCodeRuleInput, CodeRegexDto>
{
    public override void Configure()
    {
        Post("/sys/code-rule/regex");
        Summary(summary => summary.Summary = "按模板生成反向正则");
        Options(options => options.WithTags("编码规则")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.CodeRule.Preview));
    }

    public override Task HandleAsync(SaveCodeRuleInput request, CancellationToken ct)
    {
        Response = new CodeRegexDto { Pattern = service.GenerateRegex(request) };
        return Task.CompletedTask;
    }
}
