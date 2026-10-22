namespace Axis.Domain.Enums;

/// <summary>
/// 收件箱消息来源类型（统一用户消息中心）。
/// </summary>
/// <remarks>
/// 1–9 预留给系统内置；10+ 预留给业务模块扩展（报警、工单等）。
/// 前端跳转优先使用 <c>LinkPath</c> + <c>BizId</c>，不必为每种业务改枚举分支。
/// </remarks>
public enum InboxSourceTypeEnum
{
    /// <summary>未知。</summary>
    Unknown = 0,

    /// <summary>通知公告。</summary>
    Notice = 1,

    /// <summary>站内信。</summary>
    Message = 2,

    /// <summary>系统通知（平台级）。</summary>
    System = 9,

    /// <summary>业务报警（预留）。</summary>
    Alarm = 10,

    /// <summary>运维工单（预留）。</summary>
    WorkOrder = 20
}
