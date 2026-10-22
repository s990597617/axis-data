using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
///     系统菜单（目录 / 菜单 / 按钮三级）
/// </summary>
[SugarTable("sys_menu", "系统菜单")]
[SysTable]
[SugarIndex("index_{table}_pid", nameof(ParentId), OrderByType.Asc)]
[SugarIndex("index_{table}_perm", nameof(Permission), OrderByType.Asc)]
public class SysMenu : BaseEntity
{
    /// <summary>
    ///     父级菜单 Id（顶级为 0）
    /// </summary>
    [SugarColumn(ColumnDescription = "父级Id")]
    public long ParentId { get; set; }

    /// <summary>
    ///     菜单类型
    /// </summary>
    [SugarColumn(ColumnDescription = "菜单类型")]
    public MenuTypeEnum Type { get; set; } = MenuTypeEnum.Menu;

    /// <summary>
    ///     菜单名称
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "菜单名称", Length = 64)]
    public string Title { get; set; } = null!;

    /// <summary>
    ///     路由名称（Vue 路由 name，唯一）
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "路由名称", Length = 64, IsNullable = true)]
    public string Name { get; set; }

    /// <summary>
    ///     路由路径
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "路由路径", Length = 128, IsNullable = true)]
    public string Path { get; set; }

    /// <summary>
    ///     组件路径
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "组件路径", Length = 128, IsNullable = true)]
    public string Component { get; set; }

    /// <summary>
    ///     权限标识（按钮 / 接口鉴权使用，如：sys:user:add）
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "权限标识", Length = 128, IsNullable = true)]
    public string Permission { get; set; }

    /// <summary>
    ///     图标
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "图标", Length = 64, IsNullable = true)]
    public string Icon { get; set; }

    /// <summary>
    ///     外链地址（Type = Menu 且 IsFrame = true 时使用）
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "外链地址", Length = 256, IsNullable = true)]
    public string Redirect { get; set; }

    /// <summary>
    ///     是否外链
    /// </summary>
    [SugarColumn(ColumnDescription = "是否外链")]
    public bool IsFrame { get; set; }

    /// <summary>
    ///     是否缓存
    /// </summary>
    [SugarColumn(ColumnDescription = "是否缓存")]
    public bool IsKeepAlive { get; set; } = true;

    /// <summary>
    ///     是否隐藏
    /// </summary>
    [SugarColumn(ColumnDescription = "是否隐藏")]
    public bool IsHide { get; set; }


    /// <summary>
    ///     是否平台专属（不可纳入租户菜单启用集，如租户管理）。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否平台专属")]
    public bool IsPlatform { get; set; }

    /// <summary>
    ///     排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    ///     状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    ///     备注
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "备注", Length = 256, IsNullable = true)]
    public string Remark { get; set; }
}