using Axis.Domain.Enums;

namespace Axis.Application.Demo.OutboxPing.Dto;

/// <summary>
/// Outbox 探测结果。
/// </summary>
public sealed class OutboxPingDto
{
    /// <summary>
    /// 事件 Id。
    /// </summary>
    public long EventId { get; init; }

    /// <summary>
    /// 当前投递状态。
    /// </summary>
    public OutboxStatusEnum Status { get; init; }

    /// <summary>
    /// 最近错误；成功时为空。
    /// </summary>
    public string? LastError { get; init; }
}
