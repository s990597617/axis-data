using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database.Permission;
using Axis.Infrastructure.Dependency.Annotations;
using SqlSugar;

namespace Axis.Application.System.DataScope;

/// <summary>
/// 按当前用户角色解析数据权限范围（供 <see cref="SqlSugarFilter.SetOrgEntityFilter"/> 使用）。
/// </summary>
/// <remarks>
/// 过滤器由中间件调用 <see cref="SqlSugarFilter"/> 挂载；本服务只负责解析范围快照。
/// </remarks>
public sealed class DataScopeService(
    ISqlSugarClient db,
    ICurrentAccount currentAccount)
    : IDataScopeLoader, IAutoInject
{
    private DataScopeState? _cached;

    /// <inheritdoc />
    public async Task<DataScopeState> LoadAsync(CancellationToken ct = default)
    {
        if (_cached is not null) return _cached;

        var userId = currentAccount.Id;
        if (userId <= 0)
        {
            _cached = DataScopeState.UnrestrictedState;
            return _cached;
        }

        if (currentAccount.IsSuperAdmin)
        {
            _cached = new DataScopeState { UserId = userId, Unrestricted = true };
            return _cached;
        }

        var roles = await db.Queryable<SysUserRole, SysRole>((ur, role) => ur.RoleId == role.Id)
            .ClearFilter()
            .Where((ur, role) => ur.UserId == userId && role.Status == StatusEnum.Enable)
            .Select((ur, role) => new { role.Id, role.DataScope })
            .ToListAsync(ct);

        if (roles.Count == 0)
        {
            _cached = new DataScopeState { UserId = userId, IncludeSelf = true };
            return _cached;
        }

        if (roles.Any(item => item.DataScope == DataScopeEnum.All))
        {
            _cached = new DataScopeState { UserId = userId, Unrestricted = true };
            return _cached;
        }

        var orgIds = new HashSet<long>();
        var includeSelf = false;
        var customRoleIds = new List<long>();
        var needDeptChildren = false;

        foreach (var role in roles)
        {
            switch (role.DataScope)
            {
                case DataScopeEnum.Custom:
                    customRoleIds.Add(role.Id);
                    break;
                case DataScopeEnum.Dept:
                    if (currentAccount.OrgId > 0) orgIds.Add(currentAccount.OrgId);
                    break;
                case DataScopeEnum.DeptWithChildren:
                    needDeptChildren = true;
                    break;
                case DataScopeEnum.Self:
                    includeSelf = true;
                    break;
            }
        }

        if (customRoleIds.Count > 0)
        {
            var customOrgIds = await db.Queryable<SysRoleOrg>()
                .ClearFilter()
                .Where(item => customRoleIds.Contains(item.RoleId))
                .Select(item => item.OrgId)
                .ToListAsync(ct);
            foreach (var orgId in customOrgIds)
                if (orgId > 0) orgIds.Add(orgId);
        }

        if (needDeptChildren && currentAccount.OrgId > 0)
        {
            var children = await db.Queryable<SysOrg>()
                .ClearFilter()
                .ToChildListAsync(item => item.ParentId, currentAccount.OrgId);
            foreach (var org in children)
                orgIds.Add(org.Id);
        }

        _cached = new DataScopeState
        {
            UserId = userId,
            OrgIds = orgIds.ToList(),
            IncludeSelf = includeSelf
        };
        return _cached;
    }
}
