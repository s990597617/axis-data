namespace Axis.Domain.Enums;

/// <summary>
/// 通知公告发布范围。
/// </summary>
public enum NoticeTargetTypeEnum
{
    /// <summary>全部用户。</summary>
    All = 0,

    /// <summary>指定角色。</summary>
    Role = 1,

    /// <summary>指定机构。</summary>
    Org = 2,

    /// <summary>指定用户。</summary>
    User = 3
}
