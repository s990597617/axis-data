using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Infrastructure.Components.Database.Permission;

/// <summary>
/// SqlSugar 数据权限全局过滤器（接口级）。
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><see cref="IUserDataPermission"/>：用户表（机构 + 本人 Id）</item>
/// <item><see cref="IOrgId"/>：仅按所属机构</item>
/// <item><see cref="IOrgCreatedBy"/>：机构 或 创建人</item>
/// <item><see cref="IOrgOwner"/>：机构 或 业务负责人/发布人</item>
/// </list>
/// 机构主表 <c>SysOrg</c> 由 Application 层按具体类型挂过滤。
/// 更复杂规则通过 <see cref="IDataPermissionContributor"/> 扩展。
/// </remarks>
public static class DataPermission
{
    /// <summary>
    /// 在 SqlSugar 连接初始化时预留扩展点。
    /// </summary>
    public static void Register(SqlSugarScopeProvider db) => _ = db;

    /// <summary>
    /// 按当前用户数据范围写入接口级 QueryFilter。
    /// </summary>
    /// <returns>是否实际附加了过滤条件（无限制时为 false）。</returns>
    public static bool Apply(ISqlSugarClient db, DataScopeState scope)
    {
        ArgumentNullException.ThrowIfNull(db);
        scope ??= DataScopeState.UnrestrictedState;

        Clear(db);

        if (scope.Unrestricted) return false;

        var orgIds = scope.OrgIds as List<long> ?? scope.OrgIds.ToList();
        var userId = scope.UserId;
        var includeSelf = scope.IncludeSelf;
        var hasOrg = orgIds.Count > 0;

        ApplyUserFilter(db, orgIds, userId, includeSelf, hasOrg);
        ApplyOrgIdFilter(db, orgIds, hasOrg);
        ApplyOrgCreatedByFilter(db, orgIds, userId, includeSelf, hasOrg);
        ApplyOrgOwnerFilter(db, orgIds, userId, includeSelf, hasOrg);

        return true;
    }

    /// <summary>
    /// 清除接口级数据权限过滤器。
    /// </summary>
    public static void Clear(ISqlSugarClient db)
    {
        if (db?.QueryFilter is null) return;

        TryClear<IUserDataPermission>(db);
        TryClear<IOrgId>(db);
        TryClear<IOrgCreatedBy>(db);
        TryClear<IOrgOwner>(db);
    }

    private static void ApplyUserFilter(
        ISqlSugarClient db,
        List<long> orgIds,
        long userId,
        bool includeSelf,
        bool hasOrg)
    {
        if (hasOrg && includeSelf)
            db.QueryFilter.AddTableFilter<IUserDataPermission>(u => orgIds.Contains(u.OrgId) || u.Id == userId);
        else if (hasOrg)
            db.QueryFilter.AddTableFilter<IUserDataPermission>(u => orgIds.Contains(u.OrgId));
        else if (includeSelf)
            db.QueryFilter.AddTableFilter<IUserDataPermission>(u => u.Id == userId);
        else
            db.QueryFilter.AddTableFilter<IUserDataPermission>(_ => false);
    }

    private static void ApplyOrgIdFilter(ISqlSugarClient db, List<long> orgIds, bool hasOrg)
    {
        if (hasOrg)
            db.QueryFilter.AddTableFilter<IOrgId>(item => orgIds.Contains(item.OrgId));
        else
            db.QueryFilter.AddTableFilter<IOrgId>(_ => false);
    }

    private static void ApplyOrgCreatedByFilter(
        ISqlSugarClient db,
        List<long> orgIds,
        long userId,
        bool includeSelf,
        bool hasOrg)
    {
        // 创建人维度始终允许看自己创建的；角色含 Self 或有机构范围时均可 OR
        if (hasOrg)
            db.QueryFilter.AddTableFilter<IOrgCreatedBy>(item =>
                orgIds.Contains(item.OrgId) || item.CreatedBy == userId);
        else if (includeSelf)
            db.QueryFilter.AddTableFilter<IOrgCreatedBy>(item => item.CreatedBy == userId);
        else
            db.QueryFilter.AddTableFilter<IOrgCreatedBy>(_ => false);
    }

    private static void ApplyOrgOwnerFilter(
        ISqlSugarClient db,
        List<long> orgIds,
        long userId,
        bool includeSelf,
        bool hasOrg)
    {
        if (hasOrg)
            db.QueryFilter.AddTableFilter<IOrgOwner>(item =>
                orgIds.Contains(item.OrgId) || item.OwnerUserId == userId);
        else if (includeSelf)
            db.QueryFilter.AddTableFilter<IOrgOwner>(item => item.OwnerUserId == userId);
        else
            db.QueryFilter.AddTableFilter<IOrgOwner>(_ => false);
    }

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
