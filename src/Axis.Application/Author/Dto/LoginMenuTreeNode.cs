using Axis.Domain.Enums;

namespace Axis.Application.Author.Dto;

/// <summary>
/// 登录后下发给前端的菜单树节点（对齐 Vue 路由结构）。
/// </summary>
/// <remarks>
/// 仅包含目录与菜单；按钮节点已剔除，权限由 <see cref="LoginUserInfo.Buttons"/> 单独下发。
/// </remarks>
public sealed class LoginMenuTreeNode
{
    /// <summary>
    /// 菜单节点主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 父节点 Id；根节点为 0。
    /// </summary>
    public long Pid { get; set; }

    /// <summary>
    /// 节点类型：目录 / 菜单（不含按钮）。
    /// </summary>
    public MenuTypeEnum Type { get; set; }

    /// <summary>
    /// 前端路由 name。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 前端路由 path。
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// 前端组件路径。
    /// </summary>
    public string Component { get; set; }

    /// <summary>
    /// 权限标识；目录/菜单节点可为空。
    /// </summary>
    public string Permission { get; set; }

    /// <summary>
    /// 重定向地址。
    /// </summary>
    public string Redirect { get; set; }

    /// <summary>
    /// 排序号。
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 启用状态。
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    public string Remark { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 最后修改时间。
    /// </summary>
    public DateTime? UpdateTime { get; set; }

    /// <summary>
    /// 路由元数据。
    /// </summary>
    public LoginMenuMeta Meta { get; set; } = new();

    /// <summary>
    /// 子节点列表。
    /// </summary>
    public List<LoginMenuTreeNode> Children { get; set; } = [];
}
