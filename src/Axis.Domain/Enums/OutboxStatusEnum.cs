namespace Axis.Domain.Enums;

/// <summary>
/// Outbox 投递状态。
/// </summary>
/// <remarks>
/// 发布后不得改已有成员数值。
/// </remarks>
public enum OutboxStatusEnum
{
    /// <summary>
    /// 未知或未指定。
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// 待投递。
    /// </summary>
    Pending = 1,

    /// <summary>
    /// 已投递成功。
    /// </summary>
    Published = 2,

    /// <summary>
    /// 投递失败，等待重试。
    /// </summary>
    Failed = 3,

    /// <summary>
    /// 超过重试上限，进入死信。
    /// </summary>
    Dead = 4
}
