using Axis.Domain.Enums;

namespace Axis.Application.System.Notice.Dto;

/// <summary>
/// 通知公告管理端分页查询条件。
/// </summary>
public sealed class NoticePageQuery
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
    /// 关键字；匹配标题，可为空表示不按关键字过滤。
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 公告类型筛选；为空表示不限。
    /// </summary>
    public NoticeTypeEnum? NoticeType { get; set; }

    /// <summary>
    /// 启用状态筛选；为空表示不限。
    /// </summary>
    public StatusEnum? Status { get; set; }
}
