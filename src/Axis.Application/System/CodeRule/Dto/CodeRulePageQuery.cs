using Axis.Domain.Enums;

namespace Axis.Application.System.CodeRule.Dto;

/// <summary>
/// 编码规则分页查询条件。
/// </summary>
public sealed class CodeRulePageQuery
{
    /// <summary>
    /// 页码，从 1 开始。
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页条数；默认 20。
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 关键字；同时模糊匹配名称与编码。
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 来源类型；为空表示不按来源过滤。
    /// </summary>
    public CodeSourceTypeEnum? SourceType { get; set; }

    /// <summary>
    /// 启用状态；为空表示不按状态过滤。
    /// </summary>
    public StatusEnum? Status { get; set; }
}
