using Axis.Domain.Enums;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.System;

namespace Axis.Infrastructure.Data.System.Repository;

/// <summary>
/// 角色仓储实现
/// </summary>
public sealed class SysRoleRepository(ISqlSugarClient db, IServiceProvider sp)
    : BaseRepository<SysRole>(db, sp), ISysRoleRepository, IAutoInject
{
    public async Task<IReadOnlyList<string>> GetEnabledCodesAsync(IReadOnlyCollection<long> roleIds, CancellationToken ct = default)
    {
        if (roleIds.Count == 0) return Array.Empty<string>();

        return await Context.Queryable<SysRole>()
            .Where(r => roleIds.Contains(r.Id) && r.Status == StatusEnum.Enable)
            .Select(r => r.Code)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<long>> GetMenuIdsAsync(IReadOnlyCollection<long> roleIds, CancellationToken ct = default)
    {
        if (roleIds.Count == 0) return Array.Empty<long>();

        return await Context.Queryable<SysRoleMenu>()
            .Where(x => roleIds.Contains(x.RoleId))
            .Select(x => x.MenuId)
            .Distinct()
            .ToListAsync(ct);
    }
}
