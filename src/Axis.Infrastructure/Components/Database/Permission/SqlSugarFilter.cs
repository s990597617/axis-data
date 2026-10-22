using System.Security.Claims;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Author.Internal;
using Axis.Infrastructure.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace Axis.Infrastructure.Components.Database.Permission;

/// <summary>
/// SqlSugar 全局查询过滤器：软删除、租户隔离与机构数据范围。
/// </summary>
/// <remarks>
/// 挂载在各 <c>GetConnectionScope</c> 连接上；请求开始时 Clear 后按当前用户重挂，避免单例连接残留上一请求条件。
/// </remarks>
public static class SqlSugarFilter
{
    private static IHttpContextAccessor? _httpContextAccessor;

    /// <summary>
    /// 注入 HttpContext 访问器（SqlSugar 注册时调用一次）。
    /// </summary>
    public static void Configure(IHttpContextAccessor? accessor) => _httpContextAccessor = accessor;

    /// <summary>
    /// 配置软删除过滤器：自动附加 <c>Deleted == false</c>。
    /// </summary>
    /// <remarks>
    /// 默认对所有用户生效（含超管）；需要查看已删数据时请在查询上 <c>ClearFilter&lt;ISoftDelete&gt;()</c>。
    /// </remarks>
    public static void SetDeletedEntityFilter(SqlSugarScopeProvider db)
    {
        ArgumentNullException.ThrowIfNull(db);
        TryClear<ISoftDelete>(db);
        db.QueryFilter.AddTableFilter<ISoftDelete>(u => u.Deleted == false);
    }

    /// <summary>
    /// 配置租户过滤器。
    /// </summary>
    /// <remarks>
    /// 启动建库时通常无登录用户会跳过；已认证请求由数据权限中间件再次调用。
    /// </remarks>
    public static void SetTenantEntityFilter(SqlSugarScopeProvider db)
    {
        ArgumentNullException.ThrowIfNull(db);
        ClearTenantFilters(db);

        var (skip, tenantId) = ResolveTenantId();
        if (skip || tenantId is null)
            return;

        var id = tenantId.Value;
        db.QueryFilter.AddTableFilter<ITenantId>(u => u.TenantId == id);
        db.QueryFilter.AddTableFilter<ITenantOrGlobal>(u => u.TenantId == null || u.TenantId == id);
    }

    /// <summary>
    /// 对租户下全部连接重挂软删除与租户过滤（Job / Outbox 进入作用域后调用）。
    /// </summary>
    /// <param name="sugar">SqlSugar 客户端。</param>
    public static void RebindTenantFilters(ISqlSugarClient? sugar)
    {
        if (sugar is null) return;
        foreach (var provider in EnumerateProviders(sugar))
        {
            SetDeletedEntityFilter(provider);
            SetTenantEntityFilter(provider);
        }
    }

    /// <summary>
    /// 配置机构数据范围过滤器。
    /// </summary>
    public static void SetOrgEntityFilter(SqlSugarScopeProvider db)
    {
        ArgumentNullException.ThrowIfNull(db);
        ClearOrgEntityFilter(db);

        if (IsSuperAdmin(CurrentUser()))
            return;

        var scope = DataScopeContext.Current;
        // 全部数据或尚未解析范围时不限制机构
        if (scope is null || scope.Unrestricted)
            return;

        var orgIds = scope.OrgIds as List<long> ?? scope.OrgIds.ToList();
        var userId = scope.UserId;
        var includeSelf = scope.IncludeSelf;
        var hasOrg = orgIds.Count > 0;

        // 仅本人
        if (!hasOrg && includeSelf)
        {
            db.QueryFilter.AddTableFilter<IUserDataPermission>(u => u.Id == userId);
            db.QueryFilter.AddTableFilter<IOrgId>(_ => false);
            db.QueryFilter.AddTableFilter<IOrgCreatedBy>(item => item.CreatedBy == userId);
            db.QueryFilter.AddTableFilter<IOrgOwner>(item => item.OwnerUserId == userId);
            return;
        }

        if (!hasOrg)
        {
            db.QueryFilter.AddTableFilter<IUserDataPermission>(_ => false);
            db.QueryFilter.AddTableFilter<IOrgId>(_ => false);
            db.QueryFilter.AddTableFilter<IOrgCreatedBy>(_ => false);
            db.QueryFilter.AddTableFilter<IOrgOwner>(_ => false);
            return;
        }

        if (includeSelf)
            db.QueryFilter.AddTableFilter<IUserDataPermission>(u => orgIds.Contains(u.OrgId) || u.Id == userId);
        else
            db.QueryFilter.AddTableFilter<IUserDataPermission>(u => orgIds.Contains(u.OrgId));

        db.QueryFilter.AddTableFilter<IOrgId>(item => orgIds.Contains(item.OrgId));
        db.QueryFilter.AddTableFilter<IOrgCreatedBy>(item =>
            orgIds.Contains(item.OrgId) || item.CreatedBy == userId);
        db.QueryFilter.AddTableFilter<IOrgOwner>(item =>
            orgIds.Contains(item.OrgId) || item.OwnerUserId == userId);
    }

    /// <summary>
    /// 配置业务扩展过滤器（<see cref="IDataPermissionContributor"/>）。
    /// </summary>
    public static void SetCustomEntityFilter(
        SqlSugarScopeProvider db,
        IEnumerable<IDataPermissionContributor> contributors)
    {
        ArgumentNullException.ThrowIfNull(db);
        var scope = DataScopeContext.Current ?? DataScopeState.UnrestrictedState;
        foreach (var contributor in contributors)
        {
            contributor.Clear(db);
            if (!IsSuperAdmin(CurrentUser()) && scope is { Unrestricted: false })
                contributor.Apply(db, scope);
        }
    }

    /// <summary>
    /// 清除机构数据范围过滤器。
    /// </summary>
    public static void ClearOrgEntityFilter(ISqlSugarClient db)
    {
        if (db?.QueryFilter is null) return;
        TryClear<IUserDataPermission>(db);
        TryClear<IOrgId>(db);
        TryClear<IOrgCreatedBy>(db);
        TryClear<IOrgOwner>(db);
    }

    private static void ClearTenantFilters(ISqlSugarClient db)
    {
        if (db?.QueryFilter is null) return;
        TryClear<ITenantId>(db);
        TryClear<ITenantOrGlobal>(db);
    }

    /// <summary>
    /// 解析当前应过滤的租户：作用域覆盖优先于 HTTP 用户。
    /// </summary>
    private static (bool Skip, long? TenantId) ResolveTenantId()
    {
        var scoped = TenantScopeContext.Current;
        if (scoped is not null)
            return scoped.Unrestricted || scoped.TenantId is null
                ? (true, null)
                : (false, scoped.TenantId);

        var user = CurrentUser();
        if (IsSuperAdmin(user))
            return (true, null);

        var raw = user?.FindFirst(ClaimConst.TenantId)?.Value;
        if (string.IsNullOrWhiteSpace(raw) || !long.TryParse(raw, out var id))
            return (true, null);

        return (false, id);
    }

    private static IEnumerable<SqlSugarScopeProvider> EnumerateProviders(ISqlSugarClient sugar)
    {
        var tenant = sugar.AsTenant();
        var configs = GlobalContext.BootstrapDb?.ConnectionConfigs;
        if (configs is null || configs.Count == 0)
            yield break;

        foreach (var config in configs)
        {
            var configId = config.ConfigId?.ToString();
            if (string.IsNullOrWhiteSpace(configId) || !tenant.IsAnyConnection(configId))
                continue;

            yield return tenant.GetConnectionScope(configId);
        }
    }

    private static ClaimsPrincipal? CurrentUser()
        => _httpContextAccessor?.HttpContext?.User;

    private static bool IsSuperAdmin(ClaimsPrincipal? user)
        => user?.FindFirst(ClaimConst.AccountType)?.Value
           == ((int)AccountTypeEnum.SuperAdmin).ToString();

    private static void TryClear<T>(ISqlSugarClient db)
    {
        try
        {
            db.QueryFilter.Clear<T>();
        }
        catch (ArgumentNullException)
        {
            // QueryFilter 内部列表未初始化时忽略
        }
    }
}

/// <summary>
/// 当前请求数据范围（供 <see cref="SqlSugarFilter.SetOrgEntityFilter"/> 使用）。
/// </summary>
public static class DataScopeContext
{
    private static readonly AsyncLocal<DataScopeState?> CurrentState = new();

    /// <summary>
    /// 当前请求数据范围。
    /// </summary>
    public static DataScopeState? Current
    {
        get => CurrentState.Value;
        set => CurrentState.Value = value;
    }
}