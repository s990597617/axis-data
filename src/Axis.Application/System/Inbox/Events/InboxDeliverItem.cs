using Axis.Domain.Enums;

namespace Axis.Application.System.Inbox.Events;

/// <summary>
/// 单收件人投递项；用于每人载荷或来源不同的追加写入。
/// </summary>
/// <remarks>
/// 通常配合 <see cref="InboxDeliverModeEnum.Append"/>。
/// 填入 <see cref="InboxDeliverEvent.Items"/> 时，广播的 UserIds/Payload 应留空。
/// </remarks>
public sealed class InboxDeliverItem
{
    /// <summary>
    /// 收件人用户 Id；必须大于 0，否则该条被忽略。
    /// </summary>
    public long UserId { get; init; }

    /// <summary>
    /// 该收件人独立的消息载荷，含各自的 SourceType/SourceId。
    /// </summary>
    public InboxMessagePayload Payload { get; init; } = null!;
}
