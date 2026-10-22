using Axis.Domain.Enums;

namespace Axis.Application.System.OpenApiIdentity.Dto;

/// <summary>
/// 开放接口身份分页查询。
/// </summary>
public sealed class OpenApiIdentityPageQuery
{
    /// <summary>页码，从 1 开始。</summary>
    public int Page { get; set; } = 1;

    /// <summary>每页条数。</summary>
    public int PageSize { get; set; } = 20;

    /// <summary>关键字：匹配 AccessKey / 备注。</summary>
    public string? Keyword { get; set; }

    /// <summary>状态筛选。</summary>
    public StatusEnum? Status { get; set; }
}
