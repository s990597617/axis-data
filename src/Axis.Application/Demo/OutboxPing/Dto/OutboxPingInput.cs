namespace Axis.Application.Demo.OutboxPing.Dto;

/// <summary>
/// Outbox 探测入参。
/// </summary>
public sealed class OutboxPingInput
{
    /// <summary>
    /// 探测文案。
    /// </summary>
    public string Message { get; set; } = "ping";

    /// <summary>
    /// 入队后抛错，用于验证事务回滚后 Outbox 无残留。
    /// </summary>
    public bool ThrowAfterEnqueue { get; set; }
}
