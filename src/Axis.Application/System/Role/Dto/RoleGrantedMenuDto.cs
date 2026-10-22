using Axis.Domain.Enums;

namespace Axis.Application.System.Role.Dto;

/// <summary>
/// 角色详情中已授权的菜单或按钮节点。
/// </summary>
/// <remarks>
/// 仅包含该角色实际授权的节点，并按父子关系组装为树；不返回未授权菜单。
/// </remarks>
public sealed class RoleGrantedMenuDto
{
    /// <summary>
    /// 菜单或按钮主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 上级节点 Id；顶级为 0。
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 节点类型：目录 / 菜单 / 按钮。
    /// </summary>
    public MenuTypeEnum Type { get; set; }

    /// <summary>
    /// 显示名称。
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 权限标识；目录或路由菜单可为空。
    /// </summary>
    public string? Permission { get; set; }

    /// <summary>
    /// 子节点；无子级时为空列表。
    /// </summary>
    public List<RoleGrantedMenuDto> Children { get; set; } = [];
}
