namespace Axis.Domain.Enums;

/// <summary>
/// 已废弃：请使用 <see cref="InboxSourceTypeEnum"/>。
/// </summary>
[Obsolete("使用 InboxSourceTypeEnum")]
public enum InboxItemTypeEnum
{
    /// <summary>通知公告。</summary>
    Notice = 1,

    /// <summary>站内信。</summary>
    Message = 2
}
