namespace Axis.Infrastructure.Domain.Interfaces;

/// <summary>
/// 业务数据权限：所属机构 + 创建人。
/// </summary>
/// <remarks>
/// 过滤条件为 <c>OrgId ∈ 可访问机构 OR CreatedBy == 当前用户</c>。
/// 与 <see cref="IOrgId"/> 互斥：实现本接口的实体不要再实现 <see cref="IOrgId"/>。
/// </remarks>
public interface IOrgCreatedBy
{
    /// <summary>
    /// 所属机构 Id。
    /// </summary>
    long OrgId { get; set; }

    /// <summary>
    /// 创建人用户 Id。
    /// </summary>
    long CreatedBy { get; set; }
}
