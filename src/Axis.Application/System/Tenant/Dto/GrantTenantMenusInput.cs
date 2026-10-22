namespace Axis.Application.System.Tenant.Dto;

/// <summary>
/// 保存租户菜单启用集的输入。
/// </summary>
public sealed class GrantTenantMenusInput
{
    /// <summary>
    /// 租户主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 要启用的菜单与按钮 Id；空集合表示清空。
    /// </summary>
    public IReadOnlyList<long> MenuIds { get; set; } = Array.Empty<long>();
}
