using Axis.Domain.Enums;

namespace Axis.Application.System.Inbox.Dto;

/// <summary>
/// 收件箱分页查询条件。
/// </summary>
public sealed class InboxPageQuery
{
    /// <summary>
    /// 页码，从 1 开始；默认 1。
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页条数；默认 20。
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 关键字；仅匹配标题与摘要（列表不搜正文，避免 text 字段 LIKE）。
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 来源类型筛选；为空表示不限。
    /// </summary>
    public InboxSourceTypeEnum? SourceType { get; set; }

    /// <summary>
    /// 已读状态筛选；为空表示不限。
    /// </summary>
    public bool? IsRead { get; set; }
}
