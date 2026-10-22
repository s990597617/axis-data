namespace Axis.Application.Demo.OutboxPing.Dto;

/// <summary>
/// 按事件 Id 查询 Outbox 探测行。
/// </summary>
public sealed class OutboxPingIdRequest
{
    /// <summary>
    /// 事件 Id。
    /// </summary>
    public long EventId { get; set; }
}
