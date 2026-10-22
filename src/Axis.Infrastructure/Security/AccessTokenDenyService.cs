using Axis.Infrastructure.Components.Caching.Services;
using Axis.Infrastructure.Constants;
using Axis.Infrastructure.Dependency.Annotations;

namespace Axis.Infrastructure.Security;

/// <summary>
/// 基于 <see cref="ICacheService"/> 的 AccessToken 拒绝名单实现。
/// </summary>
/// <remarks>
/// 缓存键前缀见 <see cref="CacheCatalog.AccessTokenDeny.KeyPrefix"/>；值为占位整数，仅以键是否存在表达吊销状态。
/// TTL = 令牌剩余有效期，避免拒绝名单无限膨胀。
/// </remarks>
/// <param name="cache">分布式/本地缓存服务。</param>
public sealed class AccessTokenDenyService(ICacheService cache) : IAccessTokenDenyService, IAutoInject
{
    /// <inheritdoc />
    public async Task DenyAsync(string jti, DateTime accessTokenExpiresAt, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jti)) return;

        var ttl = accessTokenExpiresAt - DateTime.Now;
        if (ttl <= TimeSpan.Zero) return;

        await cache.SetAsync(BuildKey(jti), 1, ttl, ct);
    }

    /// <inheritdoc />
    public async Task DenyManyAsync(
        IEnumerable<(string Jti, DateTime ExpiresAt)> items,
        CancellationToken ct = default)
    {
        foreach (var (jti, expiresAt) in items)
            await DenyAsync(jti, expiresAt, ct);
    }

    /// <inheritdoc />
    public Task<bool> IsDeniedAsync(string jti, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jti)) return Task.FromResult(false);
        return cache.ExistsAsync(BuildKey(jti), ct);
    }

    /// <summary>
    /// 组装拒绝名单缓存键。
    /// </summary>
    /// <param name="jti">JWT Id。</param>
    /// <returns>带统一前缀的缓存键。</returns>
    private static string BuildKey(string jti) => $"{CacheCatalog.AccessTokenDeny.KeyPrefix}{jti}";
}
