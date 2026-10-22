namespace Axis.Application.System.Inbox.Dto;

/// <summary>
/// 收件箱单条操作请求（详情 / 已读）；主键为收件箱行 Id。
/// </summary>
public sealed class InboxItemRequest
{
    /// <summary>
    /// 收件箱主键。
    /// </summary>
    public long Id { get; set; }
}
