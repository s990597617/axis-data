using Axis.Domain.Data.System.Cache;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Constants;
using ICacheService = Axis.Infrastructure.Components.Caching.Services.ICacheService;

namespace Axis.Infrastructure.Data.System.Cache;

/// <summary>
/// 编码规则定义缓存实现。
/// </summary>
/// <remarks>
/// 键形如 <c>axis:sys:code-rule:item:{scope}:{code}</c>；流水禁止进入本缓存。
/// </remarks>
public sealed class SysCodeRuleCache(ISqlSugarClient db, ICacheService cache, ICurrentAccount currentAccount)
    : ISysCodeRuleCache, IAutoInject
{
    /// <summary>
    /// 按规则编码读取启用中的规则快照。
    /// </summary>
    /// <param name="code">规则编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>启用规则；不存在或已禁用时为 null。</returns>
    public async Task<CodeRuleCacheItem?> GetEnabledAsync(string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        var key = BuildItemKey(code);
        var cached = await cache.GetAsync<CodeRuleCacheItem>(key, ct);
        if (cached is not null)
            return cached.Status == StatusEnum.Enable ? cached : null;

        var item = await LoadByCodeAsync(code, ct);
        if (item is not null)
            await cache.SetAsync(key, item, CacheCatalog.MetaCacheTtl, ct);
        return item is { Status: StatusEnum.Enable } ? item : null;
    }

    /// <summary>
    /// 当前租户下全部启用规则，按优先级升序。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>启用规则列表。</returns>
    public async Task<IReadOnlyList<CodeRuleCacheItem>> ListEnabledAsync(CancellationToken ct = default)
    {
        var key = BuildListKey();
        var cached = await cache.GetAsync<List<CodeRuleCacheItem>>(key, ct);
        if (cached is not null) return cached;

        var list = await LoadEnabledListAsync(ct);
        await cache.SetAsync(key, list, CacheCatalog.MetaCacheTtl, ct);
        return list;
    }

    /// <summary>
    /// 写库成功后刷新指定编码的缓存。
    /// </summary>
    /// <param name="code">规则编码。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task RefreshAsync(string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return;
        var item = await LoadByCodeAsync(code, ct);
        var key = BuildItemKey(code);
        if (item is null)
            await cache.RemoveAsync(key, ct);
        else
            await cache.SetAsync(key, item, CacheCatalog.MetaCacheTtl, ct);
        await cache.RemoveAsync(BuildListKey(), ct);
    }

    /// <summary>
    /// 失效指定编码及启用列表缓存。
    /// </summary>
    /// <param name="code">规则编码。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task InvalidateAsync(string code, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(code))
            await cache.RemoveAsync(BuildItemKey(code), ct);
        await cache.RemoveAsync(BuildListKey(), ct);
    }

    /// <summary>
    /// 回源加载单条规则（含禁用项，供刷新写入）。
    /// </summary>
    private async Task<CodeRuleCacheItem?> LoadByCodeAsync(string code, CancellationToken ct)
    {
        var tenantId = currentAccount.TenantId;
        return await db.Queryable<SysCodeRule>()
            .Where(item => item.Code == code)
            .WhereIF(tenantId.HasValue, item => item.TenantId == tenantId)
            .Select(item => new CodeRuleCacheItem
            {
                Id = item.Id,
                TenantId = item.TenantId,
                Code = item.Code,
                Name = item.Name,
                Prefix = item.Prefix,
                DateFormat = item.DateFormat,
                SerialLength = item.SerialLength,
                ResetCycle = item.ResetCycle,
                Template = item.Template,
                CheckAlgo = item.CheckAlgo,
                SourceType = item.SourceType,
                ReverseRegex = item.ReverseRegex,
                Priority = item.Priority,
                Status = item.Status
            }, true)
            .FirstAsync(ct);
    }

    /// <summary>
    /// 回源加载当前租户启用规则列表。
    /// </summary>
    private async Task<List<CodeRuleCacheItem>> LoadEnabledListAsync(CancellationToken ct)
    {
        var tenantId = currentAccount.TenantId;
        return await db.Queryable<SysCodeRule>()
            .Where(item => item.Status == StatusEnum.Enable)
            .WhereIF(tenantId.HasValue, item => item.TenantId == tenantId)
            .OrderBy(item => item.Priority)
            .OrderBy(item => item.Id)
            .Select(item => new CodeRuleCacheItem
            {
                Id = item.Id,
                TenantId = item.TenantId,
                Code = item.Code,
                Name = item.Name,
                Prefix = item.Prefix,
                DateFormat = item.DateFormat,
                SerialLength = item.SerialLength,
                ResetCycle = item.ResetCycle,
                Template = item.Template,
                CheckAlgo = item.CheckAlgo,
                SourceType = item.SourceType,
                ReverseRegex = item.ReverseRegex,
                Priority = item.Priority,
                Status = item.Status
            }, true)
            .ToListAsync(ct);
    }

    private string BuildItemKey(string code)
    {
        var scope = currentAccount.TenantId?.ToString() ?? CacheCatalog.GlobalScope;
        return cache.GenerateKey(CacheCatalog.CodeRule.KeyPrefix, scope, code);
    }

    private string BuildListKey()
    {
        var scope = currentAccount.TenantId?.ToString() ?? CacheCatalog.GlobalScope;
        return cache.GenerateKey(CacheCatalog.CodeRuleList.KeyPrefix, scope);
    }
}
