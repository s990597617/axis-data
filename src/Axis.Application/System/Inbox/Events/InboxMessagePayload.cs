using Axis.Domain.Enums;

namespace Axis.Application.System.Inbox.Events;

/// <summary>
/// 收件箱投递事件的消息内容载荷。
/// </summary>
/// <remarks>
/// 由发布方组装；内容为写入 <c>sys_inbox</c> 的快照字段。
/// 列表摘要为空时，Handler 会按正文截断生成 Summary。
/// </remarks>
public sealed class InboxMessagePayload
{
    /// <summary>
    /// 标题快照；写入前会 Trim，不能为空字符串。
    /// </summary>
    public string Title { get; init; } = null!;

    /// <summary>
    /// 列表摘要；可空。为空时由正文截断生成。
    /// </summary>
    public string Summary { get; init; }

    /// <summary>
    /// 正文快照；可空。详情页展示用，业务跳转场景可只填摘要。
    /// </summary>
    public string Content { get; init; }

    /// <summary>
    /// 消息来源类型；与 <see cref="SourceId"/> 组成来源键。
    /// </summary>
    public InboxSourceTypeEnum SourceType { get; init; }

    /// <summary>
    /// 来源业务主键（公告 Id、站内信 Id 等）；无来源时为 0。
    /// </summary>
    public long SourceId { get; init; }

    /// <summary>
    /// 业务模块编码（如 notice / alarm）；可空，供前端分发。
    /// </summary>
    public string BizModule { get; init; }

    /// <summary>
    /// 业务子类型编码；可空。
    /// </summary>
    public string BizType { get; init; }

    /// <summary>
    /// 业务单据 Id，供跳转占位；可与 SourceId 相同，可空。
    /// </summary>
    public long? BizId { get; init; }

    /// <summary>
    /// 前端跳转路径；可含 <c>{bizId}</c> 占位符，可空。
    /// </summary>
    public string LinkPath { get; init; }

    /// <summary>
    /// 扩展 JSON 载荷；避免频繁加列，可空。
    /// </summary>
    public string ExtraJson { get; init; }

    /// <summary>
    /// 发件人用户 Id；系统代发可为 0。
    /// </summary>
    public long SenderId { get; init; }

    /// <summary>
    /// 发件人显示名快照；可空。
    /// </summary>
    public string SenderName { get; init; }

    /// <summary>
    /// 优先级；数值越大列表越靠前，默认 0。
    /// </summary>
    public int Priority { get; init; }
}
