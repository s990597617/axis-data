using Axis.Domain.Data.System.Cache;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.System;
using Axis.Infrastructure.Constants;
using ICacheService = Axis.Infrastructure.Components.Caching.Services.ICacheService;

namespace Axis.Infrastructure.Data.System.Cache;

/// <summary>
/// 用户按钮权限码缓存门面实现。
/// </summary>
/// <remarks>
/// 键形如 <c>{GlobalPrefix}:sys:user:perm:{userId}</c>；值与
/// <see cref="ISysMenuRepository.GetButtonPermissionsAsync"/> 一致。
/// </remarks>
public sealed class UserPermissionCache(
    ISysMenuRepository menuRepo,
    ICacheService cache,
    ISqlSugarClient db)
    : IUserPermissionCache, IAutoInject
{
    /// <summary>
    /// 获取用户按钮权限码；未命中时回源并回填。
    /// </summary>
    /// <param name="userId">用户主键。</param>
    /// <param name="isSuperAdmin">是否超级管理员。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>权限码列表。</returns>
    public async Task<IReadOnlyList<string>> GetAsync(long userId, bool isSuperAdmin, CancellationToken ct = default)
    {
        if (userId <= 0) return [];

        var key = BuildKey(userId);
        // EasyCaching 带锁的 Get+factory，避免同一用户并发鉴权击穿回源。
        var codes = await cache.GetAsync(
            key,
            async () => (await menuRepo.GetButtonPermissionsAsync(userId, isSuperAdmin, ct)).ToList(),
            CacheCatalog.UserPermissionCacheTtl,
            ct);

        return codes ?? [];
    }

    /// <summary>
    /// 将已知权限码写入缓存。
    /// </summary>
    /// <param name="userId">用户主键。</param>
    /// <param name="codes">权限码集合。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task SetAsync(long userId, IReadOnlyList<string> codes, CancellationToken ct = default)
    {
        if (userId <= 0) return;

        var value = codes is List<string> list
            ? list
            : codes.ToList();
        await cache.SetAsync(BuildKey(userId), value, CacheCatalog.UserPermissionCacheTtl, ct);
    }

    /// <summary>
    /// 从数据库重新加载并写穿刷新。
    /// </summary>
    /// <param name="userId">用户主键。</param>
    /// <param name="isSuperAdmin">是否超级管理员。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task RefreshAsync(long userId, bool isSuperAdmin, CancellationToken ct = default)
    {
        if (userId <= 0) return;

        var codes = await menuRepo.GetButtonPermissionsAsync(userId, isSuperAdmin, ct);
        await SetAsync(userId, codes, ct);
    }

    /// <summary>
    /// 失效指定用户权限缓存。
    /// </summary>
    /// <param name="userId">用户主键。</param>
    /// <param name="ct">取消令牌。</param>
    public Task InvalidateAsync(long userId, CancellationToken ct = default)
        => userId <= 0
            ? Task.CompletedTask
            : cache.RemoveAsync(BuildKey(userId), ct);

    /// <summary>
    /// 角色菜单授权变更后，失效该角色下全部用户权限缓存。
    /// </summary>
    /// <param name="roleId">角色主键。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task InvalidateByRoleAsync(long roleId, CancellationToken ct = default)
    {
        if (roleId <= 0) return;

        var userIds = await db.Queryable<SysUserRole>()
            .Where(item => item.RoleId == roleId)
            .Select(item => item.UserId)
            .Distinct()
            .ToListAsync(ct);

        await RemoveUsersAsync(userIds, ct);
    }

    /// <summary>
    /// 租户菜单启用集变更后，失效该租户下全部用户权限缓存。
    /// </summary>
    /// <param name="tenantId">租户主键。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task InvalidateByTenantAsync(long tenantId, CancellationToken ct = default)
    {
        if (tenantId <= 0) return;

        var userIds = await db.Queryable<SysUser>()
            .Where(item => item.TenantId == tenantId)
            .Select(item => item.Id)
            .ToListAsync(ct);

        await RemoveUsersAsync(userIds, ct);
    }

    private async Task RemoveUsersAsync(IReadOnlyList<long> userIds, CancellationToken ct)
    {
        if (userIds.Count == 0) return;
        await cache.RemoveManyAsync(userIds.Select(BuildKey), ct);
    }

    private string BuildKey(long userId)
        => cache.GenerateKey(CacheCatalog.UserPermission.KeyPrefix, userId);
}
