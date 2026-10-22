namespace Axis.Infrastructure.Components.Database.Permission;

/// <summary>
/// 当前请求解析后的数据权限快照，供 SqlSugar QueryFilter 使用。
/// </summary>
public sealed class DataScopeState
{
    /// <summary>
    /// 未登录或无需过滤时的空快照（不附加条件）。
    /// </summary>
    public static DataScopeState UnrestrictedState { get; } = new() { Unrestricted = true };

    /// <summary>
    /// 当前用户 Id。
    /// </summary>
    public long UserId { get; init; }

    /// <summary>
    /// 是否不限制数据范围。
    /// </summary>
    public bool Unrestricted { get; init; }

    /// <summary>
    /// 可访问机构 Id。
    /// </summary>
    public IReadOnlyList<long> OrgIds { get; init; } = [];

    /// <summary>
    /// 是否包含本人数据。
    /// </summary>
    public bool IncludeSelf { get; init; }

    /// <summary>
    /// 判断是否可访问指定机构。
    /// </summary>
    public bool CanAccessOrg(long orgId)
        => Unrestricted || OrgIds.Contains(orgId);

    /// <summary>
    /// 判断是否可访问指定用户。
    /// </summary>
    public bool CanAccessUser(long userId, long orgId)
        => Unrestricted || OrgIds.Contains(orgId) || (IncludeSelf && userId == UserId);
}
