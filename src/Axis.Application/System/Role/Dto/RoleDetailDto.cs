using Axis.Domain.Enums;

namespace Axis.Application.System.Role.Dto;

/// <summary>
/// 角色详情聚合数据：主表字段 + 已授权菜单树。
/// </summary>
/// <remarks>
/// 一次返回详情页所需全部数据，避免前端再拆请求菜单列表与授权 Id。
/// 超级管理员 <c>OwnAllPermissions</c> 为 true 时，<see cref="Menus"/> 为空，由前端展示说明文案。
/// </remarks>
public sealed class RoleDetailDto
{
    /// <summary>
    /// 角色主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 乐观锁版本号。
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// 角色显示名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 角色编码；租户内唯一。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 数据权限范围枚举值。
    /// </summary>
    public DataScopeEnum DataScope { get; set; }

    /// <summary>
    /// 自定义数据范围勾选的机构 Id；非自定义时为空列表。
    /// </summary>
    public List<long> OrgIds { get; set; } = [];

    /// <summary>
    /// 自定义机构名称（与 <see cref="OrgIds"/> 对应）；非自定义时为空列表。
    /// </summary>
    public List<string> OrgNames { get; set; } = [];

    /// <summary>
    /// 排序号。
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

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 最后修改时间；从未修改时为空。
    /// </summary>
    public DateTime? ModifiedTime { get; set; }

    /// <summary>
    /// 是否固定拥有全部权限（超级管理员）；为 true 时不必枚举菜单树。
    /// </summary>
    public bool OwnAllPermissions { get; set; }

    /// <summary>
    /// 已授权菜单树；超级管理员时为空列表。
    /// </summary>
    public IReadOnlyList<RoleGrantedMenuDto> Menus { get; set; } = [];
}
