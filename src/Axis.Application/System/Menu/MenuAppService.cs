using Axis.Application.Common;
using Axis.Application.System.Menu.Dto;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Permissions;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using SqlSugar;
using Yitter.IdGenerator;

namespace Axis.Application.System.Menu;

/// <summary>
/// 菜单管理应用服务。
/// </summary>
/// <remarks>
/// 负责菜单、目录与按钮节点维护；HTTP 由 Endpoint 处理。
/// </remarks>
public sealed class MenuAppService(
    IRepository<SysMenu> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount)
    : BaseAppService<SysMenu>(repository, db, currentAccount), IMenuAppService, IAutoInject
{
    /// <summary>
    /// 按条件查询菜单节点平铺列表，投影为 <see cref="MenuDto"/>。
    /// </summary>
    /// <param name="query">筛选条件；关键字同时匹配标题与权限标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>菜单 DTO 只读列表，由前端组装树。</returns>
    public async Task<IReadOnlyList<MenuDto>> ListAsync(MenuListQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item =>
                item.Title.Contains(query.Keyword!) ||
                (item.Permission != null && item.Permission.Contains(query.Keyword!)))
            .WhereIF(query.Type.HasValue, item => item.Type == query.Type!.Value)
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .Select(item => new MenuDto
            {
                Id = item.Id,
                ParentId = item.ParentId,
                Type = item.Type,
                Title = item.Title,
                Name = item.Name,
                Path = item.Path,
                Component = item.Component,
                Permission = item.Permission,
                Icon = item.Icon,
                Redirect = item.Redirect,
                IsFrame = item.IsFrame,
                IsKeepAlive = item.IsKeepAlive,
                IsHide = item.IsHide,
                IsPlatform = item.IsPlatform,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark
            })
            .ToListAsync(ct);

    /// <summary>
    /// 获取系统权限码目录（按模块分组），供菜单按钮表单选择。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>模块分组列表。</returns>
    public Task<IReadOnlyList<PermissionCatalogModuleDto>> GetPermissionCatalogAsync(
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var modules = PermissionCatalog.All
            .GroupBy(item => new { item.Module, item.ModuleName, item.ModuleOrder })
            .OrderBy(group => group.Key.ModuleOrder)
            .ThenBy(group => group.Key.Module)
            .Select(group => new PermissionCatalogModuleDto
            {
                Module = group.Key.Module,
                ModuleName = group.Key.ModuleName,
                Items = group
                    .OrderBy(item => item.ActionOrder)
                    .ThenBy(item => item.Code)
                    .Select(item => new PermissionCatalogItemDto
                    {
                        Code = item.Code,
                        ActionName = item.ActionName
                    })
                    .ToList()
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<PermissionCatalogModuleDto>>(modules);
    }

    /// <summary>
    /// 校验父级与唯一性后创建菜单节点。
    /// </summary>
    /// <param name="input">新增字段；页面菜单须填路由，按钮须填权限标识。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">字段不合规、父级不存在或唯一性冲突时抛出。</exception>
    public async Task CreateAsync(SaveMenuInput input, CancellationToken ct = default)
    {
        ValidateInput(input);
        await EnsureParentExistsAsync(input.ParentId, ct);
        await EnsureUniqueAsync(input, null, ct);

        await Db.Insertable(new SysMenu
        {
            Id = YitIdHelper.NextId(),
            ParentId = input.ParentId,
            Type = input.Type,
            Title = input.Title,
            Name = input.Name,
            Path = input.Path,
            Component = input.Component,
            Permission = input.Permission,
            Icon = input.Icon,
            Redirect = input.Redirect,
            IsFrame = input.IsFrame,
            IsKeepAlive = input.IsKeepAlive,
            IsHide = input.IsHide,
            IsPlatform = input.IsPlatform,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = DateTime.Now
        }).ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 更新指定菜单节点；禁止把节点移动到自身后代下。
    /// </summary>
    /// <param name="id">菜单节点主键。</param>
    /// <param name="input">更新字段。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">节点不存在、形成环或唯一性冲突时抛出。</exception>
    public async Task UpdateAsync(long id, SaveMenuInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("菜单节点不存在");
        ValidateInput(input);
        await EnsureParentExistsAsync(input.ParentId, ct);
        await EnsureNoCycleAsync(id, input.ParentId, ct);
        await EnsureUniqueAsync(input, id, ct);

        entity.ParentId = input.ParentId;
        entity.Type = input.Type;
        entity.Title = input.Title;
        entity.Name = input.Name;
        entity.Path = input.Path;
        entity.Component = input.Component;
        entity.Permission = input.Permission;
        entity.Icon = input.Icon;
        entity.Redirect = input.Redirect;
        entity.IsFrame = input.IsFrame;
        entity.IsKeepAlive = input.IsKeepAlive;
        entity.IsHide = input.IsHide;
        entity.IsPlatform = input.IsPlatform;
        entity.OrderNo = input.OrderNo;
        entity.Status = input.Status;
        entity.Remark = input.Remark;
        entity.ModifiedBy = CurrentAccount.Id;
        entity.Modifier = CurrentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;
        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);
    }

    /// <summary>
    /// 删除菜单节点；存在子节点或已分配角色时不允许删除。
    /// </summary>
    /// <param name="id">菜单节点主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">存在子节点、已授权或节点不存在时抛出。</exception>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var hasChildren = await Repo.AsQueryable().AnyAsync(item => item.ParentId == id, ct);
        if (hasChildren) throw new BusinessException("请先删除当前节点的子节点");

        var isGranted = await Db.Queryable<SysRoleMenu>().AnyAsync(item => item.MenuId == id, ct);
        if (isGranted) throw new BusinessException("菜单已分配给角色，请先调整角色授权");

        var affected = await Db.Deleteable<SysMenu>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("菜单节点不存在");
    }

    /// <summary>
    /// 根据节点类型校验路由字段与权限字段。
    /// </summary>
    /// <param name="input">待校验输入。</param>
    /// <exception cref="BusinessException">类型对应必填字段缺失时抛出。</exception>
    private static void ValidateInput(SaveMenuInput input)
    {
        if (input.Type == MenuTypeEnum.Btn)
        {
            if (string.IsNullOrWhiteSpace(input.Permission))
                throw new BusinessException("按钮节点必须填写权限标识");
            if (!PermissionCatalog.Contains(input.Permission))
                throw new BusinessException($"权限标识 '{input.Permission}' 不在系统权限目录中，请从目录选择");
        }
    }

    /// <summary>
    /// 校验非根父节点存在。
    /// </summary>
    /// <param name="parentId">父节点 Id；0 表示根。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">父节点不存在时抛出。</exception>
    private async Task EnsureParentExistsAsync(long parentId, CancellationToken ct)
    {
        if (parentId == 0) return;
        if (!await Repo.AsQueryable().AnyAsync(item => item.Id == parentId, ct))
            throw new BusinessException("父级菜单不存在");
    }

    /// <summary>
    /// 沿父级链向上检查，防止把节点移动到自身后代下形成环。
    /// </summary>
    /// <param name="id">当前节点 Id。</param>
    /// <param name="parentId">目标父节点 Id。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">形成环时抛出。</exception>
    private async Task EnsureNoCycleAsync(long id, long parentId, CancellationToken ct)
    {
        var nodes = await Repo.AsQueryable().Select(item => new { item.Id, item.ParentId }).ToListAsync(ct);
        var parents = nodes.ToDictionary(item => item.Id, item => item.ParentId);
        var cursor = parentId;
        while (cursor != 0 && parents.TryGetValue(cursor, out var next))
        {
            if (cursor == id) throw new BusinessException("父级菜单不能选择当前节点或其后代");
            cursor = next;
        }
    }

    /// <summary>
    /// 校验路由名称和权限标识的唯一性。
    /// </summary>
    /// <param name="input">待校验输入。</param>
    /// <param name="excludedId">更新时排除的节点 Id；新增时为空。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">名称或权限标识冲突时抛出。</exception>
    private async Task EnsureUniqueAsync(SaveMenuInput input, long? excludedId, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(input.Name))
        {
            var nameExists = await Repo.AsQueryable()
                .Where(item => item.Name == input.Name)
                .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
                .AnyAsync(ct);
            if (nameExists) throw new BusinessException($"路由名称 '{input.Name}' 已存在");
        }

        if (!string.IsNullOrWhiteSpace(input.Permission))
        {
            var permissionExists = await Repo.AsQueryable()
                .Where(item => item.Permission == input.Permission)
                .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
                .AnyAsync(ct);
            if (permissionExists) throw new BusinessException($"权限标识 '{input.Permission}' 已存在");
        }
    }
}
