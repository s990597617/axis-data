using Axis.Domain.Enums;

namespace Axis.Application.System.Menu.Dto;

/// <summary>
/// 菜单、目录和按钮的统一传输结构。
/// </summary>
/// <remarks>
/// 仅暴露管理端展示与编辑所需字段，不直接返回领域实体。
/// </remarks>
public sealed class MenuDto
{
    /// <summary>
    /// 菜单节点主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 父节点 Id；根节点为 0。
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 节点类型：目录 / 菜单 / 按钮。
    /// </summary>
    public MenuTypeEnum Type { get; set; }

    /// <summary>
    /// 显示标题。
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 前端路由名称；页面菜单时使用。
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 前端路由路径；页面菜单时使用。
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// 前端组件路径；可为空。
    /// </summary>
    public string? Component { get; set; }

    /// <summary>
    /// 权限标识；按钮节点必填。
    /// </summary>
    public string? Permission { get; set; }

    /// <summary>
    /// 图标标识；可为空。
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 重定向地址；可为空。
    /// </summary>
    public string? Redirect { get; set; }

    /// <summary>
    /// 是否外链。
    /// </summary>
    public bool IsFrame { get; set; }

    /// <summary>
    /// 是否页面缓存。
    /// </summary>
    public bool IsKeepAlive { get; set; }

    /// <summary>
    /// 是否在菜单中隐藏。
    /// </summary>
    public bool IsHide { get; set; }

    /// <summary>
    /// 是否平台专属（不可分配给租户）。
    /// </summary>
    public bool IsPlatform { get; set; }

    /// <summary>
    /// 排序号；数值越小越靠前。
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 启用状态。
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// 备注；可为空。
    /// </summary>
    public string? Remark { get; set; }
}
