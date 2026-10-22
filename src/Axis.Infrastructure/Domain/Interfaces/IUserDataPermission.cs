namespace Axis.Infrastructure.Domain.Interfaces;

/// <summary>
/// 用户类数据权限实体（按机构 + 可选仅本人过滤）。
/// </summary>
/// <remarks>
/// 实现该接口的实体由 SqlSugar 全局 QueryFilter 自动附加数据范围条件。
/// </remarks>
public interface IUserDataPermission
{
    /// <summary>
    /// 主键 Id（仅本人范围时使用）。
    /// </summary>
    long Id { get; set; }

    /// <summary>
    /// 所属机构 Id。
    /// </summary>
    long OrgId { get; set; }
}
