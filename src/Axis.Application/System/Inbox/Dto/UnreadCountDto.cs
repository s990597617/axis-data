namespace Axis.Application.System.Inbox.Dto;

/// <summary>
/// 当前用户未读统计（铃铛角标与 SignalR 推送）。
/// </summary>
public sealed class UnreadCountDto
{
    /// <summary>
    /// 未读总数。
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 通知公告未读数。
    /// </summary>
    public int NoticeCount { get; set; }

    /// <summary>
    /// 消息类未读数（SourceType=Message）。
    /// </summary>
    public int MessageCount { get; set; }

    /// <summary>
    /// 其它业务消息未读数（报警/工单等）。
    /// </summary>
    public int OtherCount { get; set; }
}
