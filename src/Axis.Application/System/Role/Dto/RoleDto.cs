using Axis.Domain.Enums;

namespace Axis.Application.System.Role.Dto;

/// <summary>
/// 角色列表项及编辑回显数据。
/// </summary>
/// <remarks>
/// 仅暴露管理端展示与编辑所需字段，不直接返回领域实体。
/// </remarks>
public sealed class RoleDto
{
    /// <summary>
    /// 角色主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 乐观锁版本号；编辑提交时原样回传。
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// 角色显示名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 角色编码；租户内唯一，用于内置角色识别（如 super-admin）。
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

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 最后修改时间；从未修改时为空。
    /// </summary>
    public DateTime? ModifiedTime { get; set; }
}
