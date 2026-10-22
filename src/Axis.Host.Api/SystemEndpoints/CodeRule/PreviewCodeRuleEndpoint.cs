using Axis.Application.System.CodeRule;
using Axis.Application.System.CodeRule.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.CodeRule;

/// <summary>
/// 按未保存表单预览编码规则。
/// </summary>
public sealed class PreviewCodeRuleEndpoint(ICodeRuleAppService service)
    : AxisEndpoint<SaveCodeRuleInput, CodePreviewDto>
{
    public override void Configure()
    {
        Post("/sys/code-rule/preview");
        Summary(summary => summary.Summary = "预览编码规则");
        Options(options => options.WithTags("编码规则")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.CodeRule.Preview));
    }

    public override Task HandleAsync(SaveCodeRuleInput request, CancellationToken ct)
    {
        Response = service.Preview(request);
        return Task.CompletedTask;
    }
}
