using Axis.Application.Common;
using Axis.Application.System.Menu.Dto;
using Axis.Application.System.Role.Dto;
using Axis.Domain.Data.System.Cache;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using MapsterMapper;
using SqlSugar;
using Yitter.IdGenerator;

namespace Axis.Application.System.Role;

/// <summary>
/// 角色管理应用服务。
/// </summary>
/// <remarks>
/// 负责角色主表维护、自定义数据范围机构与菜单授权；HTTP / Cookie 由 Endpoint 处理。
/// 自定义机构校验依赖 SqlSugar 全局机构 QueryFilter。
/// </remarks>
public sealed class RoleAppService(
    IRepository<SysRole> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount,
    IUserPermissionCache permissionCache,
    IMapper mapper)
    : BaseAppService<SysRole>(repository, db, currentAccount), IRoleAppService, IAutoInject
{
    /// <summary>
    /// 按条件分页查询角色列表，投影为 <see cref="RoleDto"/>，并附带自定义机构 Id。
    /// </summary>
    /// <param name="query">分页与筛选条件；关键字同时匹配名称与编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>角色 DTO 分页结果。</returns>
    public async Task<SqlSugarPagedList<RoleDto>> PageAsync(RolePageQuery query, CancellationToken ct = default)
    {
        var page = await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item =>
                item.Name.Contains(query.Keyword!) || item.Code.Contains(query.Keyword!))
            .WhereIF(query.DataScope.HasValue, item => item.DataScope == query.DataScope!.Value)
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new RoleDto(), true)
            .ToPagedListAsync(query.Page, query.PageSize);

        await FillOrgIdsAsync(page.Items, ct);
        return page;
    }

    /// <summary>
    /// 查询角色详情，并附带已授权菜单树与自定义机构。
    /// </summary>
    /// <param name="id">角色主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>详情聚合 DTO；角色不存在时为 null。</returns>
    public async Task<RoleDetailDto?> GetDetailAsync(long id, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct);
        if (entity is null) return null;

        var detail = mapper.Map<RoleDetailDto>(entity);
        detail.OrgIds = await LoadOrgIdsAsync(id, ct);
        if (detail.OrgIds.Count > 0)
        {
            detail.OrgNames = await Db.Queryable<SysOrg>()
                .Where(item => detail.OrgIds.Contains(item.Id))
                .OrderBy(item => item.OrderNo)
                .Select(item => item.Name)
                .ToListAsync(ct);
        }

        if (entity.Code == "super-admin")
        {
            detail.OwnAllPermissions = true;
            detail.Menus = [];
            return detail;
        }

        var menus = await Db.Queryable<SysRoleMenu, SysMenu>((rm, menu) => rm.MenuId == menu.Id)
            .Where((rm, menu) => rm.RoleId == id)
            .OrderBy((rm, menu) => menu.OrderNo)
            .Select((rm, menu) => new RoleGrantedMenuDto
            {
                Id = menu.Id,
                ParentId = menu.ParentId,
                Type = menu.Type,
                Title = menu.Title,
                Permission = menu.Permission
            })
            .ToListAsync(ct);

        detail.OwnAllPermissions = false;
        detail.Menus = BuildGrantedMenuTree(menus);
        return detail;
    }

    /// <summary>
    /// 校验编码唯一后创建角色主表记录，并按数据范围同步机构关联。
    /// </summary>
    /// <param name="input">新增角色字段；编码须在当前租户内唯一。</param>
    /// <param name="ct">取消令牌。</param>
    /// <remarks>不处理菜单授权；授权见 <see cref="GrantMenusAsync"/>。</remarks>
    /// <exception cref="BusinessException">编码已存在或机构越权时抛出。</exception>
    public async Task CreateAsync(SaveRoleInput input, CancellationToken ct = default)
    {
        await EnsureCodeUniqueAsync(input.Code, null, ct);
        var orgIds = await NormalizeOrgIdsAsync(input, ct);

        var entity = mapper.Map<SysRole>(input);
        entity.Id = 0;
        entity.Version = 0;
        entity.TenantId = CurrentAccount.TenantId;

        await Db.Insertable(entity).ExecuteCommandAsync(ct);
        await ReplaceRoleOrgsAsync(entity.Id, orgIds, ct);
    }

    /// <summary>
    /// 更新指定角色的主表字段，并按数据范围同步机构关联。
    /// </summary>
    /// <param name="id">要更新的角色主键。</param>
    /// <param name="input">更新字段；编码须在当前租户内唯一（排除自身）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">角色不存在、编码冲突或机构越权时抛出。</exception>
    public async Task UpdateAsync(long id, SaveRoleInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("角色不存在");
        await EnsureCodeUniqueAsync(input.Code, id, ct);
        var orgIds = await NormalizeOrgIdsAsync(input, ct);

        mapper.Map(input, entity);
        entity.Version = input.Version;
        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);
        await ReplaceRoleOrgsAsync(id, orgIds, ct);
    }

    /// <summary>
    /// 删除角色及其菜单、机构关联；已分配用户或超级管理员角色不允许删除。
    /// </summary>
    /// <param name="id">要删除的角色主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">角色不存在、为超级管理员或仍绑定用户时抛出。</exception>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("角色不存在");
        if (entity.Code == "super-admin") throw new BusinessException("超级管理员角色不允许删除");

        var hasUsers = await Db.Queryable<SysUserRole>().AnyAsync(item => item.RoleId == id, ct);
        if (hasUsers) throw new BusinessException("角色已分配给用户，不能删除");

        await Db.Deleteable<SysRoleMenu>().Where(item => item.RoleId == id).ExecuteCommandAsync(ct);
        await Db.Deleteable<SysRoleOrg>().Where(item => item.RoleId == id).ExecuteCommandAsync(ct);
        await Db.Deleteable<SysRole>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 查询角色已分配的菜单与按钮 Id 列表。
    /// </summary>
    /// <param name="id">角色主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>菜单与按钮 Id 只读列表；无授权时为空列表。</returns>
    /// <exception cref="BusinessException">角色不存在时抛出。</exception>
    public async Task<IReadOnlyList<long>> GetMenuIdsAsync(long id, CancellationToken ct = default)
    {
        await EnsureRoleExistsAsync(id, ct);
        return await Db.Queryable<SysRoleMenu>()
            .Where(item => item.RoleId == id)
            .Select(item => item.MenuId)
            .ToListAsync(ct);
    }

    /// <summary>
    /// 覆盖保存角色的菜单与按钮授权。
    /// </summary>
    /// <param name="id">角色主键。</param>
    /// <param name="menuIds">目标菜单与按钮 Id；空集合表示清空授权。</param>
    /// <param name="ct">取消令牌。</param>
    /// <remarks>超级管理员角色不允许单独授权；菜单须 ⊆ 角色所属租户启用集。</remarks>
    /// <exception cref="BusinessException">角色不存在、为超级管理员或菜单 Id 无效时抛出。</exception>
    public async Task GrantMenusAsync(long id, IReadOnlyCollection<long> menuIds, CancellationToken ct = default)
    {
        var entity = await EnsureRoleExistsAsync(id, ct);
        if (entity.Code == "super-admin") throw new BusinessException("超级管理员固定拥有全部权限，无需单独授权");

        var distinctIds = menuIds.Where(menuId => menuId > 0).Distinct().ToArray();
        if (distinctIds.Length > 0)
        {
            var existingCount = await Db.Queryable<SysMenu>().CountAsync(menu => distinctIds.Contains(menu.Id), ct);
            if (existingCount != distinctIds.Length) throw new BusinessException("授权列表包含不存在的菜单或按钮");

            if (!entity.TenantId.HasValue)
                throw new BusinessException("角色未绑定租户，无法授权菜单");

            var enabledCount = await Db.Queryable<SysTenantMenu>()
                .Where(item => item.TenantId == entity.TenantId.Value && distinctIds.Contains(item.MenuId))
                .CountAsync(ct);
            if (enabledCount != distinctIds.Length)
                throw new BusinessException("授权列表包含租户未启用的菜单或按钮");
        }

        await Db.Deleteable<SysRoleMenu>().Where(item => item.RoleId == id).ExecuteCommandAsync(ct);
        if (distinctIds.Length == 0)
        {
            // 清空授权后也要失效，否则用户仍可能持有旧权限码。
            await permissionCache.InvalidateByRoleAsync(id, ct);
            return;
        }

        var relations = distinctIds.Select(menuId => new SysRoleMenu
        {
            Id = YitIdHelper.NextId(),
            RoleId = id,
            MenuId = menuId
        }).ToArray();
        await Db.Insertable(relations).ExecuteCommandAsync(ct);
        await permissionCache.InvalidateByRoleAsync(id, ct);
    }

    /// <summary>
    /// 查询当前租户可授予角色的菜单平铺列表。
    /// </summary>
    public async Task<IReadOnlyList<MenuDto>> ListGrantableMenusAsync(CancellationToken ct = default)
    {
        var tenantId = CurrentAccount.TenantId;
        ISugarQueryable<SysMenu> query;

        if (tenantId.HasValue)
        {
            var enabledIds = await Db.Queryable<SysTenantMenu>()
                .Where(item => item.TenantId == tenantId.Value)
                .Select(item => item.MenuId)
                .ToListAsync(ct);
            if (enabledIds.Count == 0) return [];

            query = Db.Queryable<SysMenu>().Where(menu => enabledIds.Contains(menu.Id));
        }
        else if (CurrentAccount.IsSuperAdmin)
        {
            query = Db.Queryable<SysMenu>().Where(menu => !menu.IsPlatform);
        }
        else
        {
            return [];
        }

        return await query
            .OrderBy(menu => menu.OrderNo)
            .Select(menu => new MenuDto
            {
                Id = menu.Id,
                ParentId = menu.ParentId,
                Type = menu.Type,
                Title = menu.Title,
                Name = menu.Name,
                Path = menu.Path,
                Component = menu.Component,
                Permission = menu.Permission,
                Icon = menu.Icon,
                Redirect = menu.Redirect,
                IsFrame = menu.IsFrame,
                IsKeepAlive = menu.IsKeepAlive,
                IsHide = menu.IsHide,
                IsPlatform = menu.IsPlatform,
                OrderNo = menu.OrderNo,
                Status = menu.Status,
                Remark = menu.Remark
            })
            .ToListAsync(ct);
    }

    /// <summary>
    /// 非自定义范围清空机构；自定义范围经全局机构过滤器校验后返回去重 Id。
    /// </summary>
    private async Task<long[]> NormalizeOrgIdsAsync(SaveRoleInput input, CancellationToken ct)
    {
        if (input.DataScope != DataScopeEnum.Custom) return [];

        var distinctIds = input.OrgIds.Where(id => id > 0).Distinct().ToArray();
        if (distinctIds.Length == 0) throw new BusinessException("自定义数据范围须选择至少一个机构");

        // 走全局 SysOrg 数据权限过滤器：越权机构查不到，视为非法
        var accessibleCount = await Db.Queryable<SysOrg>().CountAsync(item => distinctIds.Contains(item.Id), ct);
        if (accessibleCount != distinctIds.Length)
            throw new BusinessException("机构不存在或超出当前数据权限");

        return distinctIds;
    }

    /// <summary>
    /// 覆盖保存角色-机构关联。
    /// </summary>
    private async Task ReplaceRoleOrgsAsync(long roleId, IReadOnlyCollection<long> orgIds, CancellationToken ct)
    {
        await Db.Deleteable<SysRoleOrg>().Where(item => item.RoleId == roleId).ExecuteCommandAsync(ct);
        if (orgIds.Count == 0) return;

        var relations = orgIds.Select(orgId => new SysRoleOrg
        {
            Id = YitIdHelper.NextId(),
            RoleId = roleId,
            OrgId = orgId
        }).ToArray();
        await Db.Insertable(relations).ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 为分页结果批量填充自定义机构 Id。
    /// </summary>
    private async Task FillOrgIdsAsync(IEnumerable<RoleDto> items, CancellationToken ct)
    {
        var list = items as IList<RoleDto> ?? items.ToList();
        if (list.Count == 0) return;

        var roleIds = list.Select(item => item.Id).ToArray();
        var relations = await Db.Queryable<SysRoleOrg>()
            .Where(item => roleIds.Contains(item.RoleId))
            .Select(item => new { item.RoleId, item.OrgId })
            .ToListAsync(ct);

        var map = relations
            .GroupBy(item => item.RoleId)
            .ToDictionary(group => group.Key, group => group.Select(item => item.OrgId).ToList());

        foreach (var item in list)
            item.OrgIds = map.TryGetValue(item.Id, out var orgIds) ? orgIds : [];
    }

    private async Task<List<long>> LoadOrgIdsAsync(long roleId, CancellationToken ct)
        => await Db.Queryable<SysRoleOrg>()
            .Where(item => item.RoleId == roleId)
            .Select(item => item.OrgId)
            .ToListAsync(ct);

    /// <summary>
    /// 校验角色编码在当前租户内唯一。
    /// </summary>
    private async Task EnsureCodeUniqueAsync(string code, long? excludedId, CancellationToken ct)
    {
        var exists = await Repo.AsQueryable()
            .Where(item => item.Code == code)
            .WhereIF(CurrentAccount.TenantId.HasValue, item => item.TenantId == CurrentAccount.TenantId)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"角色编码 '{code}' 已存在");
    }

    /// <summary>
    /// 获取角色实体；不存在时抛出业务异常。
    /// </summary>
    private async Task<SysRole> EnsureRoleExistsAsync(long id, CancellationToken ct)
        => await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
           ?? throw new BusinessException("角色不存在");

    /// <summary>
    /// 将已授权的平铺菜单组装为树；父节点不在授权集合中时作为根节点挂出。
    /// </summary>
    private static IReadOnlyList<RoleGrantedMenuDto> BuildGrantedMenuTree(List<RoleGrantedMenuDto> rows)
    {
        var nodeMap = rows.ToDictionary(item => item.Id);
        var roots = new List<RoleGrantedMenuDto>();

        foreach (var node in rows)
        {
            if (node.ParentId != 0 && nodeMap.TryGetValue(node.ParentId, out var parent))
                parent.Children.Add(node);
            else
                roots.Add(node);
        }

        return roots;
    }
}
