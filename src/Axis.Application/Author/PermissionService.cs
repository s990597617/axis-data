using Axis.Domain.Data.System.Cache;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.FastEndpoints.Permission;
using Axis.Infrastructure.Dependency.Annotations;

namespace Axis.Application.Author;

/// <summary>
/// 基于当前用户按钮权限码的判定实现。
/// </summary>
/// <remarks>
/// 权限码经 <see cref="IUserPermissionCache"/> 读取（登录写入 / 未命中回源），
/// 与登录资料 <c>Buttons</c> 同源，供 <c>PermissionPreProcessor</c> 校验端点权限。
/// 同一请求内再缓存为 <see cref="HashSet{T}"/>，避免多次 HasAll/HasAny 重复读缓存。
/// </remarks>
public sealed class PermissionService(ICurrentAccount currentAccount, IUserPermissionCache permissionCache)
    : IPermissionService, IAutoInject
{
    private HashSet<string>? _ownedCodes;
    private bool _loaded;

    /// <summary>
    /// 判断当前用户是否同时具备所有给定权限码（AND）。
    /// </summary>
    /// <param name="codes">待校验权限码；空集合视为不限制，返回 <c>true</c>。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>全部命中为 <c>true</c>；未登录或任一缺失为 <c>false</c>。</returns>
    public async Task<bool> HasAllAsync(IReadOnlyList<string> codes, CancellationToken ct = default)
    {
        if (currentAccount.Id == 0) return false;
        // 超管在 PreProcessor 已短路；此处再判一次，避免服务被其它入口直接调用时漏放行。
        if (currentAccount.IsSuperAdmin) return true;
        if (codes.Count == 0) return true;

        var owned = await LoadOwnedCodesAsync(ct);

        // 仓储对超管返回 "*"；防御性兼容，视作拥有全部权限。
        return owned.Contains("*") || codes.All(owned.Contains);
    }

    /// <summary>
    /// 判断当前用户是否至少具备其中一个权限码（OR）。
    /// </summary>
    /// <param name="codes">待校验权限码；空集合视为不限制，返回 <c>true</c>。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>任一命中为 <c>true</c>；未登录或全部未命中为 <c>false</c>。</returns>
    public async Task<bool> HasAnyAsync(IReadOnlyList<string> codes, CancellationToken ct = default)
    {
        if (currentAccount.Id == 0) return false;
        if (currentAccount.IsSuperAdmin) return true;
        if (codes.Count == 0) return true;

        var owned = await LoadOwnedCodesAsync(ct);
        return owned.Contains("*") || codes.Any(owned.Contains);
    }

    /// <summary>
    /// 从分布式/本地缓存加载当前用户权限码；请求内只取一次。
    /// </summary>
    private async Task<HashSet<string>> LoadOwnedCodesAsync(CancellationToken ct)
    {
        if (_loaded) return _ownedCodes!;

        var list = await permissionCache.GetAsync(currentAccount.Id, currentAccount.IsSuperAdmin, ct);
        _ownedCodes = list.Count == 0 ? [] : new HashSet<string>(list, StringComparer.Ordinal);
        _loaded = true;
        return _ownedCodes;
    }
}