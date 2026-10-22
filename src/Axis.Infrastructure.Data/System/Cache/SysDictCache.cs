using Axis.Domain.Data.System.Cache;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Constants;
using ICacheService = Axis.Infrastructure.Components.Caching.Services.ICacheService;

namespace Axis.Infrastructure.Data.System.Cache;

/// <summary>
/// 字典缓存门面实现。
/// </summary>
/// <remarks>
/// 键形如 <c>axis:sys:dict:type:{scope}:{code}</c>：
/// <list type="bullet">
///   <item>平台字典（IsTenant=false / TenantId 为空）→ scope = <c>g</c></item>
///   <item>租户字典 → scope = 租户 Id</item>
/// </list>
/// 读取：先查当前租户键，未命中再查全局 <c>g</c>，再回源；回源结果写入**类型归属**对应的 scope（不把全局数据拷进租户键）。
/// </remarks>
public sealed class SysDictCache(ISqlSugarClient db, ICacheService cache, ICurrentAccount currentAccount)
    : ISysDictCache, IAutoInject
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<DictItemDto>> GetItemsAsync(string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return [];

        var accountTenantId = currentAccount.TenantId;
        if (accountTenantId.HasValue)
        {
            var tenantItems = await cache.GetAsync<List<DictItemDto>>(BuildKey(code, accountTenantId), ct);
            if (tenantItems is not null) return tenantItems;
        }

        var globalItems = await cache.GetAsync<List<DictItemDto>>(BuildKey(code, null), ct);
        if (globalItems is not null) return globalItems;

        var resolved = await ResolveTypeAsync(code, ct);
        if (resolved is null) return [];

        var items = await LoadItemsByTypeIdAsync(resolved.Id, ct);
        await cache.SetAsync(BuildKey(code, ResolveScopeTenantId(resolved)), items, CacheCatalog.MetaCacheTtl, ct);
        return items;
    }

    /// <inheritdoc />
    public async Task<string?> GetLabelAsync(string code, string value, CancellationToken ct = default)
    {
        var items = await GetItemsAsync(code, ct);
        return items.FirstOrDefault(item => item.Value == value)?.Label;
    }

    /// <inheritdoc />
    public async Task RefreshAsync(string code, long? tenantId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return;

        var type = await FindTypeByScopeAsync(code, tenantId, ct);
        var key = BuildKey(code, tenantId);
        if (type is null)
        {
            await cache.RemoveAsync(key, ct);
            return;
        }

        var items = await LoadItemsByTypeIdAsync(type.Id, ct);
        await cache.SetAsync(key, items, CacheCatalog.MetaCacheTtl, ct);
    }

    /// <inheritdoc />
    public async Task InvalidateAsync(string code, long? tenantId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return;
        await cache.RemoveAsync(BuildKey(code, tenantId), ct);
    }

    /// <inheritdoc />
    public async Task<int> WarmupAsync(CancellationToken ct = default)
    {
        var types = await db.Queryable<SysDictType>()
            .Where(type => type.Status == StatusEnum.Enable)
            .Select(type => new { type.Id, type.Code, type.TenantId, type.IsTenant })
            .ToListAsync(ct);

        var count = 0;
        foreach (var type in types.Where(type => !string.IsNullOrWhiteSpace(type.Code)))
        {
            var items = await LoadItemsByTypeIdAsync(type.Id, ct);
            var scopeTenantId = type.IsTenant ? type.TenantId : null;
            await cache.SetAsync(BuildKey(type.Code, scopeTenantId), items, CacheCatalog.MetaCacheTtl, ct);
            count++;
        }

        return count;
    }

    /// <summary>
    /// 按编码解析当前账号可见的字典类型：本租户优先，否则平台全局。
    /// </summary>
    private async Task<DictTypeScope?> ResolveTypeAsync(string code, CancellationToken ct)
    {
        var tenantId = currentAccount.TenantId;
        var rows = await db.Queryable<SysDictType>()
            .Where(item => item.Code == code && item.Status == StatusEnum.Enable)
            .WhereIF(tenantId.HasValue, item => item.TenantId == tenantId || item.TenantId == null)
            .WhereIF(!tenantId.HasValue, item => item.TenantId == null)
            .OrderByDescending(item => item.IsTenant)
            .OrderByDescending(item => item.TenantId != null)
            .Select(item => new DictTypeScope
            {
                Id = item.Id,
                TenantId = item.TenantId,
                IsTenant = item.IsTenant
            })
            .Take(1)
            .ToListAsync(ct);

        return rows.FirstOrDefault();
    }

    /// <summary>
    /// 按归属 scope 精确定位字典类型（写路径刷新用）。
    /// </summary>
    private async Task<DictTypeScope?> FindTypeByScopeAsync(string code, long? tenantId, CancellationToken ct)
    {
        var rows = await db.Queryable<SysDictType>()
            .Where(item => item.Code == code)
            .Where(item => item.TenantId == tenantId)
            .Select(item => new DictTypeScope
            {
                Id = item.Id,
                TenantId = item.TenantId,
                IsTenant = item.IsTenant
            })
            .Take(1)
            .ToListAsync(ct);

        return rows.FirstOrDefault();
    }

    private static long? ResolveScopeTenantId(DictTypeScope type)
        => type.IsTenant ? type.TenantId : null;

    private async Task<List<DictItemDto>> LoadItemsByTypeIdAsync(long typeId, CancellationToken ct)
        => await db.Queryable<SysDictData>()
            .Where(data => data.DictTypeId == typeId && data.Status == StatusEnum.Enable)
            .OrderBy(data => data.OrderNo)
            .Select(data => new DictItemDto
            {
                Label = data.Label,
                Value = data.Value,
                TagStyle = data.TagStyle,
                ListClass = data.ListClass,
                IsDefault = data.IsDefault,
                OrderNo = data.OrderNo
            })
            .ToListAsync(ct);

    private string BuildKey(string code, long? tenantId)
    {
        var scope = tenantId?.ToString() ?? CacheCatalog.GlobalScope;
        return cache.GenerateKey(CacheCatalog.Dict.KeyPrefix, scope, code);
    }

    private sealed class DictTypeScope
    {
        public long Id { get; init; }
        public long? TenantId { get; init; }
        public bool IsTenant { get; init; }
    }
}
