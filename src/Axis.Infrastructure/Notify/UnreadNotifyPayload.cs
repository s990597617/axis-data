namespace Axis.Infrastructure.Notify;

/// <summary>
/// 收件箱未读统计推送载荷（顶栏铃铛 / SignalR <c>UnreadChanged</c> 事件）。
/// </summary>
public sealed class UnreadNotifyPayload
{
    /// <summary>
    /// 未读总数（公告未读 + 站内信未读）。
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 通知公告未读数。
    /// </summary>
    public int NoticeCount { get; set; }

    /// <summary>
    /// 站内信未读数。
    /// </summary>
    public int MessageCount { get; set; }
}
