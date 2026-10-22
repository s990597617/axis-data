using Axis.Application.System.CodeRule;
using Axis.Application.System.CodeRule.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.FastEndpoints.Endpoints;
using Axis.Infrastructure.Components.Swagger.Enums;

namespace Axis.Host.Api.SystemEndpoints.CodeRule;

/// <summary>
/// 分页查询编码规则。
/// </summary>
public sealed class PageCodeRulesEndpoint(ICodeRuleAppService service)
    : AxisEndpoint<CodeRulePageQuery, SqlSugarPagedList<CodeRuleDto>>
{
    public override void Configure()
    {
        Get("/sys/code-rule/page");
        Summary(summary => summary.Summary = "编码规则分页列表");
        Options(options => options.WithTags("编码规则")
            .WithGroupName(nameof(ApiGroupEnum.AdminSystemModule))
            .RequirePermission(SystemPermissionCodes.CodeRule.Query));
    }

    public override async Task HandleAsync(CodeRulePageQuery request, CancellationToken ct)
        => Response = await service.PageAsync(request, ct);
}
