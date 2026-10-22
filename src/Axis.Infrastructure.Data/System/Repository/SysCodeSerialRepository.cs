using Axis.Domain.Data.System.Repository;
using Axis.Domain.System;
using Yitter.IdGenerator;

namespace Axis.Infrastructure.Data.System.Repository;

/// <summary>
/// 编码规则流水桶仓储。
/// </summary>
/// <remarks>
/// 使用行锁读取后递增，保证同事务内并发不重号。插入冲突时重试一次。
/// </remarks>
public sealed class SysCodeSerialRepository(ISqlSugarClient db, IServiceProvider sp)
    : BaseRepository<SysCodeSerial>(db, sp), ISysCodeSerialRepository, IAutoInject
{
    /// <summary>
    /// 在指定流水桶上原子加一并返回新值；桶不存在时插入 1。
    /// </summary>
    /// <param name="tenantId">取号租户。</param>
    /// <param name="ruleId">规则主键。</param>
    /// <param name="periodKey">周期键。</param>
    /// <param name="scopeKey">范围键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>占用后的流水值。</returns>
    public async Task<long> IncrementAsync(
        long? tenantId,
        long ruleId,
        string periodKey,
        string scopeKey,
        CancellationToken ct = default)
    {
        var row = await QueryBucket(tenantId, ruleId, periodKey, scopeKey)
            .TranLock(DbLockType.Wait)
            .FirstAsync(ct);

        if (row is null)
            return await InsertFirstAsync(tenantId, ruleId, periodKey, scopeKey, ct);

        row.CurrentValue += 1;
        await Context.Updateable(row)
            .UpdateColumns(item => item.CurrentValue)
            .ExecuteCommandAsync(ct);
        return row.CurrentValue;
    }

    /// <summary>
    /// 窥视下一号，不写库。
    /// </summary>
    /// <param name="tenantId">取号租户。</param>
    /// <param name="ruleId">规则主键。</param>
    /// <param name="periodKey">周期键。</param>
    /// <param name="scopeKey">范围键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>下一流水值。</returns>
    public async Task<long> PeekNextAsync(
        long? tenantId,
        long ruleId,
        string periodKey,
        string scopeKey,
        CancellationToken ct = default)
    {
        var current = await QueryBucket(tenantId, ruleId, periodKey, scopeKey)
            .Select(item => item.CurrentValue)
            .FirstAsync(ct);
        return current + 1;
    }

    /// <summary>
    /// 构造流水桶查询。
    /// </summary>
    private ISugarQueryable<SysCodeSerial> QueryBucket(
        long? tenantId,
        long ruleId,
        string periodKey,
        string scopeKey)
        => Context.Queryable<SysCodeSerial>()
            .Where(item => item.RuleId == ruleId)
            .Where(item => item.PeriodKey == periodKey)
            .Where(item => item.ScopeKey == scopeKey)
            .WhereIF(tenantId.HasValue, item => item.TenantId == tenantId)
            .WhereIF(!tenantId.HasValue, item => item.TenantId == null);

    /// <summary>
    /// 插入首条流水；并发冲突时改为锁定后递增。
    /// </summary>
    private async Task<long> InsertFirstAsync(
        long? tenantId,
        long ruleId,
        string periodKey,
        string scopeKey,
        CancellationToken ct)
    {
        try
        {
            await Context.Insertable(new SysCodeSerial
            {
                Id = YitIdHelper.NextId(),
                TenantId = tenantId,
                RuleId = ruleId,
                PeriodKey = periodKey,
                ScopeKey = scopeKey,
                CurrentValue = 1
            }).ExecuteCommandAsync(ct);
            return 1;
        }
        catch (Exception)
        {
            var row = await QueryBucket(tenantId, ruleId, periodKey, scopeKey)
                .TranLock(DbLockType.Wait)
                .FirstAsync(ct)
                ?? throw new InvalidOperationException("编码流水桶插入冲突后仍不存在。");
            row.CurrentValue += 1;
            await Context.Updateable(row)
                .UpdateColumns(item => item.CurrentValue)
                .ExecuteCommandAsync(ct);
            return row.CurrentValue;
        }
    }
}
