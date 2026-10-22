using Axis.Domain.System;

namespace Axis.Domain.Data.System.Repository;

/// <summary>
/// 角色仓储
/// </summary>
public interface ISysRoleRepository : IRepository<SysRole>
{
    /// <summary>
    /// 获取指定角色集合的编码（仅启用状态）
    /// </summary>
    Task<IReadOnlyList<string>> GetEnabledCodesAsync(IReadOnlyCollection<long> roleIds, CancellationToken ct = default);

    /// <summary>
    /// 获取角色已关联的菜单 Id 列表
    /// </summary>
    Task<IReadOnlyList<long>> GetMenuIdsAsync(IReadOnlyCollection<long> roleIds, CancellationToken ct = default);
}
