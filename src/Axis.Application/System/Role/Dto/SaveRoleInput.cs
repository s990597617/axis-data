using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.System.Role.Dto;

/// <summary>
/// 新增或更新角色的输入数据。
/// </summary>
/// <remarks>
/// 新增时 <see cref="Id"/> 由服务端忽略并重新生成；更新时由路由绑定。
/// 格式校验以 <c>SaveRoleInputValidator</c> 为准；编码唯一性由应用服务校验。
/// </remarks>
public sealed class SaveRoleInput
{
    /// <summary>
    /// 角色主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 乐观锁版本号；更新时必填（与打开编辑时一致），新增时忽略。
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// 角色显示名称；必填，最长 64。
    /// </summary>
    [Required, MaxLength(64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 角色编码；必填，最长 64，租户内唯一。
    /// </summary>
    [Required, MaxLength(64)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 数据权限范围；默认仅本人。
    /// </summary>
    public DataScopeEnum DataScope { get; set; } = DataScopeEnum.Self;

    /// <summary>
    /// 自定义数据范围勾选的机构 Id；仅 <see cref="DataScopeEnum.Custom"/> 时生效。
    /// </summary>
    public List<long> OrgIds { get; set; } = [];

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
