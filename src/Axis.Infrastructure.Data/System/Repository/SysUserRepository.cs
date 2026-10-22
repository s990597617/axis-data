using Axis.Domain.Enums;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;

namespace Axis.Infrastructure.Data.System.Repository;

/// <summary>
/// 用户仓储实现
/// </summary>
public class SysUserRepository(ISqlSugarClient db, IServiceProvider sp)
    : BaseRepository<SysUser>(db, sp), ISysUserRepository, IAutoInject
{
    public async Task<SqlSugarPagedList<SysUser>> PageWithOrgAsync(
        int page,
        int pageSize,
        long? orgId,
        string? keyword,
        StatusEnum? status,
        CancellationToken ct = default)
        => await base.AsQueryable()
            .Includes(u => u.SysOrg)
            .WhereIF(orgId.HasValue, u => u.OrgId == orgId!.Value)
            .WhereIF(!string.IsNullOrWhiteSpace(keyword), u =>
                u.Account.Contains(keyword!) ||
                u.RealName.Contains(keyword!) ||
                (u.NickName != null && u.NickName.Contains(keyword!)) ||
                (u.Phone != null && u.Phone.Contains(keyword!)))
            .WhereIF(status.HasValue, u => u.Status == status!.Value)
            .OrderBy(u => u.OrderNo)
            .OrderBy(u => u.CreatedTime, OrderByType.Desc)
            .ToPagedListAsync(page, pageSize);

    public Task<SysUser> FindByAccountAsync(string account, CancellationToken ct = default)
    {
        return base.AsQueryable().Includes(u => u.SysOrg).FirstAsync(u => u.Account == account, ct)!;
    }

    public async Task<SysUser?> FindByAccountWithOrgAsync(string account, CancellationToken ct = default)
    {
        var user = await base.AsQueryable()
            .Includes(u => u.SysOrg)
            .FirstAsync(u => u.Account == account, ct);

        return user ?? null;
    }

    public async Task<SysUser?> FindByIdWithOrgAsync(long userId, bool ignoreFilter = false, CancellationToken ct = default)
    {
        var query = base.AsQueryable().Includes(u => u.SysOrg);
        if (ignoreFilter) query = query.ClearFilter();

        var user = await query.FirstAsync(u => u.Id == userId, ct);
        return user ?? null;
    }

    public async Task<IReadOnlyList<long>> GetRoleIdsAsync(long userId, CancellationToken ct = default)
    {
        return await Context.Queryable<SysUserRole>()
            .Where(x => x.UserId == userId)
            .Select(x => x.RoleId)
            .ToListAsync(ct);
    }

    public Task UpdateLastLoginAsync(long userId, string? ip, string? device, CancellationToken ct = default)
    {
        return base.AsUpdateable()
            .SetColumns(u => new SysUser
            {
                LastLoginIp = ip,
                LastLoginDevice = device,
                LastLoginTime = DateTime.Now,
            })
            .Where(u => u.Id == userId)
            .ExecuteCommandAsync(ct);
    }
}
