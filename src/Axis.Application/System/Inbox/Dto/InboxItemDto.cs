using Axis.Domain.Enums;

namespace Axis.Application.System.Inbox.Dto;

/// <summary>
/// 收件箱列表/详情出参。
/// </summary>
public sealed class InboxItemDto
{
    /// <summary>
    /// 收件箱主键（详情 / 已读均用此 Id）。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 消息来源类型。
    /// </summary>
    public InboxSourceTypeEnum SourceType { get; set; }

    /// <summary>
    /// 来源业务主键。
    /// </summary>
    public long SourceId { get; set; }

    /// <summary>
    /// 标题。
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 摘要；详情接口返回全文时写入 Content 到本字段亦可。
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// 正文（详情）。
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 是否已读。
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// 投递时间。
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 发件人姓名。
    /// </summary>
    public string? SenderName { get; set; }

    /// <summary>
    /// 业务模块编码。
    /// </summary>
    public string? BizModule { get; set; }

    /// <summary>
    /// 业务子类型。
    /// </summary>
    public string? BizType { get; set; }

    /// <summary>
    /// 业务单据 Id。
    /// </summary>
    public long? BizId { get; set; }

    /// <summary>
    /// 跳转路径（可含 <c>{bizId}</c>）。
    /// </summary>
    public string? LinkPath { get; set; }

    /// <summary>
    /// 解析后的前端路由（已替换占位符）；无跳转时为空。
    /// </summary>
    public string? ResolvedLink { get; set; }

    /// <summary>
    /// 扩展 JSON。
    /// </summary>
    public string? ExtraJson { get; set; }

    /// <summary>
    /// 优先级。
    /// </summary>
    public int Priority { get; set; }
}
