namespace Axis.Infrastructure.Domain.Interfaces;

/// <summary>
/// 业务数据所属机构标记（仅按 <see cref="OrgId"/> 过滤）。
/// </summary>
/// <remarks>
/// 实现该接口的实体由 SqlSugar QueryFilter 自动附加 <c>OrgId ∈ 可访问机构</c>。
/// 若还需「看自己创建的 / 自己负责的」，改用 <see cref="IOrgCreatedBy"/> 或 <see cref="IOrgOwner"/>，勿同时实现。
/// </remarks>
public interface IOrgId
{
    /// <summary>
    /// 所属机构 Id
    /// </summary>
    long OrgId { get; set; }
}
