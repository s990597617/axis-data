using Axis.Domain.Data.System.Cache;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Constants;
using ICacheService = Axis.Infrastructure.Components.Caching.Services.ICacheService;

namespace Axis.Infrastructure.Data.System.Cache;

/// <summary>
/// 系统配置缓存门面实现。
/// </summary>
/// <remarks>
/// 键形如 <c>axis:sys:config:item:{scope}:{code}</c>，scope 为租户 Id 或全局标记；
/// 仅缓存启用状态配置；值为 null 时不缓存，下次读取继续回源。
/// </remarks>
public sealed class SysConfigCache(ISqlSugarClient db, ICacheService cache, ICurrentAccount currentAccount)
    : ISysConfigCache, IAutoInject
{
    /// <summary>
    /// 获取指定配置的原始字符串值。
    /// </summary>
    /// <param name="code">配置编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>配置值；配置不存在或被禁用时为 null。</returns>
    public async Task<string?> GetValueAsync(string code, CancellationToken ct = default)
    {
        var key = BuildKey(code);
        var cached = await cache.GetAsync<string>(key, ct);
        if (cached is not null) return cached;

        var value = await LoadValueAsync(code, ct);
        if (value is not null) await cache.SetAsync(key, value, CacheCatalog.MetaCacheTtl, ct);
        return value;
    }

    /// <summary>
    /// 获取整型配置值；缺失或解析失败时返回默认值。
    /// </summary>
    /// <param name="code">配置编码。</param>
    /// <param name="defaultValue">回退默认值。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>解析后的整型值或默认值。</returns>
    public async Task<int> GetIntAsync(string code, int defaultValue = 0, CancellationToken ct = default)
    {
        var value = await GetValueAsync(code, ct);
        return int.TryParse(value, out var parsed) ? parsed : defaultValue;
    }

    /// <summary>
    /// 获取布尔配置值；缺失或解析失败时返回默认值。
    /// </summary>
    /// <param name="code">配置编码。</param>
    /// <param name="defaultValue">回退默认值。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>解析后的布尔值或默认值。</returns>
    public async Task<bool> GetBoolAsync(string code, bool defaultValue = false, CancellationToken ct = default)
    {
        var value = await GetValueAsync(code, ct);
        if (string.IsNullOrWhiteSpace(value)) return defaultValue;
        if (bool.TryParse(value, out var parsed)) return parsed;

        // 兼容 1/0 数字型开关配置
        return value.Trim() switch
        {
            "1" => true,
            "0" => false,
            _ => defaultValue
        };
    }

    /// <summary>
    /// 写库成功后重新加载指定配置缓存（写穿刷新）。
    /// </summary>
    /// <param name="code">配置编码。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task RefreshAsync(string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return;
        var key = BuildKey(code);
        var value = await LoadValueAsync(code, ct);
        if (value is not null)
            await cache.SetAsync(key, value, CacheCatalog.MetaCacheTtl, ct);
        else
            await cache.RemoveAsync(key, ct);
    }

    /// <summary>
    /// 失效指定配置缓存；下次读取时懒加载回填。
    /// </summary>
    /// <param name="code">配置编码。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task InvalidateAsync(string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return;
        await cache.RemoveAsync(BuildKey(code), ct);
    }

    /// <inheritdoc />
    public async Task<int> WarmupAsync(CancellationToken ct = default)
    {
        var configs = await db.Queryable<SysConfig>()
            .Where(config => config.Status == StatusEnum.Enable)
            .Select(config => new { config.Code, config.Value, config.TenantId })
            .ToListAsync(ct);

        var count = 0;
        foreach (var config in configs)
        {
            if (string.IsNullOrWhiteSpace(config.Code) || config.Value is null) continue;
            var key = BuildKey(config.Code, config.TenantId);
            await cache.SetAsync(key, config.Value, CacheCatalog.MetaCacheTtl, ct);
            count++;
        }

        return count;
    }

    /// <summary>
    /// 回源查询指定配置的启用值。
    /// </summary>
    /// <param name="code">配置编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>配置值；不存在或禁用时为 null。</returns>
    private async Task<string?> LoadValueAsync(string code, CancellationToken ct)
    {
        var tenantId = currentAccount.TenantId;
        return await db.Queryable<SysConfig>()
            .Where(config => config.Code == code && config.Status == StatusEnum.Enable)
            .WhereIF(tenantId.HasValue, config => config.TenantId == tenantId || config.TenantId == null)
            .OrderByDescending(config => config.TenantId != null)
            .Select(config => config.Value)
            .FirstAsync(ct);
    }

    /// <summary>
    /// 构造带租户作用域的配置缓存键。
    /// </summary>
    private string BuildKey(string code) => BuildKey(code, currentAccount.TenantId);

    /// <summary>
    /// 按指定租户作用域构造配置缓存键。
    /// </summary>
    private string BuildKey(string code, long? tenantId)
    {
        var scope = tenantId?.ToString() ?? CacheCatalog.GlobalScope;
        return cache.GenerateKey(CacheCatalog.Config.KeyPrefix, scope, code);
    }
}