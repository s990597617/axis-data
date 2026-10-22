using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.System.Menu.Dto;

/// <summary>
/// 新增或更新菜单节点的输入数据。
/// </summary>
/// <remarks>
/// 新增时 <see cref="Id"/> 由服务端忽略；更新时由路由绑定。
/// 格式校验以 Validator 为准；类型相关业务规则由应用服务校验。
/// </remarks>
public sealed class SaveMenuInput
{
    /// <summary>
    /// 菜单节点主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 父节点 Id；根节点传 0。
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 节点类型；默认菜单。
    /// </summary>
    public MenuTypeEnum Type { get; set; } = MenuTypeEnum.Menu;

    /// <summary>
    /// 显示标题；必填，最长 64。
    /// </summary>
    [Required, MaxLength(64)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// 前端路由名称；目录/菜单必填，最长 64。
    /// </summary>
    [MaxLength(64)]
    public string? Name { get; set; }

    /// <summary>
    /// 前端路由路径；目录/菜单必填，最长 128。
    /// </summary>
    [MaxLength(128)]
    public string? Path { get; set; }

    /// <summary>
    /// 前端组件路径；菜单必填，最长 128。
    /// </summary>
    [MaxLength(128)]
    public string? Component { get; set; }

    /// <summary>
    /// 权限标识；按钮节点必填，最长 128。
    /// </summary>
    [MaxLength(128)]
    public string? Permission { get; set; }

    /// <summary>
    /// 图标标识；目录/菜单必填，最长 64。
    /// </summary>
    [MaxLength(64)]
    public string? Icon { get; set; }

    /// <summary>
    /// 重定向地址；可为空，最长 256。
    /// </summary>
    [MaxLength(256)]
    public string? Redirect { get; set; }

    /// <summary>
    /// 是否外链。
    /// </summary>
    public bool IsFrame { get; set; }

    /// <summary>
    /// 是否页面缓存；默认 true。
    /// </summary>
    public bool IsKeepAlive { get; set; } = true;

    /// <summary>
    /// 是否在菜单中隐藏。
    /// </summary>
    public bool IsHide { get; set; }

    /// <summary>
    /// 是否平台专属；不可纳入租户菜单启用集。
    /// </summary>
    public bool IsPlatform { get; set; }

    /// <summary>
    /// 排序号；默认 100。
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 启用状态；默认启用。
    /// </summary>
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 备注；可为空，最长 256。
    /// </summary>
    [MaxLength(256)]
    public string? Remark { get; set; }
}
