using Axis.Domain.Enums;

namespace Axis.Application.System.Notice.Dto;

/// <summary>
/// 通知公告列表/详情出参。
/// </summary>
/// <remarks>
/// 管理端分页主要使用本 DTO；用户侧收件箱使用 Inbox 模块出参，不直接复用本类型的已读字段语义。
/// </remarks>
public sealed class NoticeDto
{
    /// <summary>
    /// 公告主键。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 公告标题。
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 公告正文。
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// 公告类型。
    /// </summary>
    public NoticeTypeEnum NoticeType { get; set; }

    /// <summary>
    /// 发布范围类型。
    /// </summary>
    public NoticeTargetTypeEnum TargetType { get; set; }

    /// <summary>
    /// 目标 Id 的 JSON 数组字符串；全部范围时为空。
    /// </summary>
    public string? TargetIds { get; set; }

    /// <summary>
    /// 最近一次发布时间；从未发布时为空。
    /// </summary>
    public DateTime? PublishTime { get; set; }

    /// <summary>
    /// 列表排序号。
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 启用状态；启用表示对外可见（已发布）。
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// 备注；可为空。
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 当前用户是否已读；管理端列表通常为 false，用户侧由 Inbox 另行聚合。
    /// </summary>
    public bool IsRead { get; set; }
}
