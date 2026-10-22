namespace Axis.Application.System.Role.Dto;

/// <summary>
/// 保存角色菜单与按钮授权的输入数据。
/// </summary>
/// <remarks>
/// <see cref="Id"/> 通常由路由绑定；空列表表示清空该角色全部菜单授权。
/// </remarks>
public sealed class GrantRoleMenusInput
{
    /// <summary>
    /// 角色主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 要授予的菜单与按钮 Id 列表；允许为空表示清空授权。
    /// </summary>
    public IReadOnlyList<long> MenuIds { get; set; } = Array.Empty<long>();
}
