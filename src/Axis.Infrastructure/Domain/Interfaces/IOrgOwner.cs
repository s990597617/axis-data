namespace Axis.Infrastructure.Domain.Interfaces;

/// <summary>
/// 业务数据权限：所属机构 + 业务负责人（发布人 / 负责人等）。
/// </summary>
/// <remarks>
/// 过滤条件为 <c>OrgId ∈ 可访问机构 OR OwnerUserId == 当前用户</c>。
/// 实体把「发布人」「负责人」等字段映射为 <see cref="OwnerUserId"/> 即可复用。
/// 与 <see cref="IOrgId"/> / <see cref="IOrgCreatedBy"/> 互斥，只实现其中一种。
/// 若还需「经理可见」等复杂规则，实现 <c>IDataPermissionContributor</c>。
/// </remarks>
public interface IOrgOwner
{
    /// <summary>
    /// 所属机构 Id。
    /// </summary>
    long OrgId { get; set; }

    /// <summary>
    /// 业务归属用户 Id（发布人、负责人等）。
    /// </summary>
    long OwnerUserId { get; set; }
}
