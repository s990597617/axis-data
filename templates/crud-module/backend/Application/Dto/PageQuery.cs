__FIELD_PAGE_QUERY_USINGS__
namespace Axis.Application.__Area__.__Module__.Dto;

/// <summary>
/// __CnName__分页列表的筛选与分页条件。
/// </summary>
public sealed class __Module__PageQuery
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
    /// 关键字；模糊匹配字符串业务字段；可为空。
    /// </summary>
    public string? Keyword { get; set; }
__FIELD_PAGE_QUERY_EXTRA__
}
