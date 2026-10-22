using Axis.Domain.Data.System.Repository;
using Axis.Domain.Data.System.Repository.Dto;
using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Internal;

namespace Axis.Infrastructure.Data.System.Repository;

/// <summary>
/// 刷新令牌仓储实现
/// </summary>
public sealed class SysRefreshTokenRepository(ISqlSugarClient db, IServiceProvider sp)
    : BaseRepository<SysRefreshToken>(db, sp), ISysRefreshTokenRepository, IAutoInject
{
    public Task<SysRefreshToken?> FindByHashAsync(string tokenHash, CancellationToken ct = default)
        => base.AsQueryable()
            .FirstAsync(t => t.TokenHash == tokenHash, ct)!;

    public Task<int> TryConsumeAsync(long id, DateTime now, CancellationToken ct = default)
        => Context.Updateable<SysRefreshToken>()
            .SetColumns(t => new SysRefreshToken { IsUsed = true, UsedTime = now })
            .Where(t => t.Id == id && !t.IsUsed && t.RevokedTime == null && t.ExpiryDate > now)
            .ExecuteCommandAsync(ct);

    public Task RevokeByHashAsync(string tokenHash, DateTime now, CancellationToken ct = default)
        => Context.Updateable<SysRefreshToken>()
            .SetColumns(t => new SysRefreshToken { RevokedTime = now })
            .Where(t => t.TokenHash == tokenHash && t.RevokedTime == null)
            .ExecuteCommandAsync(ct);

    public Task<int> RevokeByIdAsync(long id, DateTime now, CancellationToken ct = default)
        => Context.Updateable<SysRefreshToken>()
            .SetColumns(t => new SysRefreshToken { RevokedTime = now })
            .Where(t => t.Id == id && t.RevokedTime == null && !t.IsUsed && t.ExpiryDate > now)
            .ExecuteCommandAsync(ct);

    public async Task<IReadOnlyList<SysRefreshToken>> RevokeByUserIdAsync(
        long userId,
        DateTime now,
        CancellationToken ct = default)
    {
        var active = await Context.Queryable<SysRefreshToken>()
            .Where(t => t.UserId == userId && t.RevokedTime == null && !t.IsUsed && t.ExpiryDate > now)
            .ToListAsync(ct);

        if (active.Count == 0) return active;

        var ids = active.Select(t => t.Id).ToArray();
        await Context.Updateable<SysRefreshToken>()
            .SetColumns(t => new SysRefreshToken { RevokedTime = now })
            .Where(t => ids.Contains(t.Id))
            .ExecuteCommandAsync(ct);

        return active;
    }

    public async Task<IReadOnlyList<SysRefreshToken>> RevokeByFamilyIdAsync(
        long familyId,
        DateTime now,
        CancellationToken ct = default)
    {
        if (familyId <= 0) return [];

        var members = await Context.Queryable<SysRefreshToken>()
            .Where(t => t.FamilyId == familyId && t.RevokedTime == null)
            .ToListAsync(ct);

        if (members.Count == 0) return members;

        var ids = members.Select(t => t.Id).ToArray();
        await Context.Updateable<SysRefreshToken>()
            .SetColumns(t => new SysRefreshToken { RevokedTime = now })
            .Where(t => ids.Contains(t.Id))
            .ExecuteCommandAsync(ct);

        return members;
    }

    public Task<SqlSugarPagedList<ActiveSessionDto>> PageActiveSessionsAsync(
        string? keyword,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var query = Context.Queryable<SysRefreshToken, SysUser>((t, u) => t.UserId == u.Id)
            .Where((t, u) => t.RevokedTime == null && !t.IsUsed && t.ExpiryDate > now)
            .WhereIF(
                !string.IsNullOrWhiteSpace(keyword),
                (t, u) =>
                    u.Account.Contains(keyword!)
                    || (u.RealName != null && u.RealName.Contains(keyword!))
                    || (t.ClientIp != null && t.ClientIp.Contains(keyword!)))
            .OrderBy((t, u) => t.CreatedTime, OrderByType.Desc)
            .Select((t, u) => new ActiveSessionDto
            {
                Id = t.Id,
                UserId = t.UserId,
                Account = u.Account,
                RealName = u.RealName,
                AccountType = (int)u.AccountType,
                AccessTokenJti = t.AccessTokenJti,
                AccessTokenExpiresAt = t.AccessTokenExpiresAt,
                ClientIp = t.ClientIp,
                UserAgent = t.UserAgent,
                CreatedTime = t.CreatedTime,
                ExpiryDate = t.ExpiryDate,
                RememberMe = t.RememberMe
            });

        return query.ToPagedListAsync(page, pageSize);
    }

    public Task<int> DeleteObsoleteAsync(DateTime cutoff, CancellationToken ct = default)
        => Context.Deleteable<SysRefreshToken>()
            .Where(t =>
                t.ExpiryDate < cutoff
                || (t.RevokedTime != null && t.RevokedTime < cutoff)
                || (t.IsUsed && t.UsedTime != null && t.UsedTime < cutoff))
            .ExecuteCommandAsync(ct);

    public Task DeleteLegacyAsync(CancellationToken ct = default)
        => Context.Deleteable<SysRefreshToken>()
            .Where(t => t.FamilyId == null || t.FamilyId == 0)
            .ExecuteCommandAsync(ct);
}
