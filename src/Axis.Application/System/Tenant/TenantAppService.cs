using Axis.Application.Common;
using Axis.Application.System.Menu.Dto;
using Axis.Application.System.Tenant.Dto;
using Axis.Domain.Data.System.Cache;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Author.Internal;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Global;
using SqlSugar;
using Yitter.IdGenerator;

namespace Axis.Application.System.Tenant;

/// <summary>
/// 租户管理应用服务：CRUD、菜单启用、Id 隔离开通管理员、独立库开通。
/// </summary>
public sealed class TenantAppService(
    IRepository<SysTenant> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount,
    IUserPermissionCache permissionCache,
    ITenantDatabaseProvisioner databaseProvisioner)
    : BaseAppService<SysTenant>(repository, db, currentAccount), ITenantAppService, IAutoInject
{
    private const string TenantAdminRoleCode = "tenant-admin";

    /// <inheritdoc />
    public async Task<SqlSugarPagedList<TenantDto>> PageAsync(TenantPageQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item =>
                item.Name.Contains(query.Keyword!) || item.Code.Contains(query.Keyword!))
            .WhereIF(query.Type.HasValue, item => item.Type == query.Type!.Value)
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new TenantDto
            {
                Id = item.Id,
                Name = item.Name,
                Code = item.Code,
                Type = item.Type,
                DbConfigId = item.DbConfigId,
                AdminUserId = item.AdminUserId,
                ProvisionStatus = item.ProvisionStatus,
                ProvisionMessage = item.ProvisionMessage,
                ExpireTime = item.ExpireTime,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime,
                ModifiedTime = item.ModifiedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);

    /// <inheritdoc />
    public async Task CreateAsync(SaveTenantInput input, CancellationToken ct = default)
    {
        await EnsureCodeUniqueAsync(input.Code, null, ct);
        ValidateDatabaseConfig(input);

        var tenantId = YitIdHelper.NextId();
        var tenant = new SysTenant
        {
            Id = tenantId,
            Name = input.Name,
            Code = input.Code,
            Type = input.Type,
            DbConfigId = input.DbConfigId,
            AdminUserId = input.AdminUserId,
            ExpireTime = input.ExpireTime,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            ProvisionStatus = input.Type == TenantTypeEnum.Db
                ? TenantProvisionStatusEnum.Pending
                : TenantProvisionStatusEnum.Ready,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = DateTime.Now
        };

        await Db.Insertable(tenant).ExecuteCommandAsync(ct);

        var menuIds = await LoadDefaultAssignableMenuIdsAsync(ct);
        await ReplaceTenantMenusAsync(tenantId, menuIds, ct);

        if (input.Type == TenantTypeEnum.Db)
        {
            var provision = await databaseProvisioner.ProvisionAsync(tenant, ct);
            tenant.ProvisionStatus = provision.Succeeded
                ? TenantProvisionStatusEnum.Ready
                : TenantProvisionStatusEnum.Failed;
            tenant.ProvisionMessage = provision.Message;
            // 列更新豁免乐观锁：开通结果回写状态字段。
            await Repo.AsUpdateable(tenant)
                .UpdateColumns(item => new { item.ProvisionStatus, item.ProvisionMessage })
                .ExecuteCommandAsync(ct);

            if (!provision.Succeeded)
                throw new BusinessException($"租户已创建，但独立库开通失败：{provision.Message}");
        }

        // Id / Schema：共享库开通租户管理员；独立库管理员仍落主库（账号路由）
        if (!input.AdminUserId.HasValue || input.AdminUserId <= 0)
            await BootstrapTenantAdminAsync(tenant, menuIds, ct);
        else
        {
            tenant.AdminUserId = input.AdminUserId;
            // 列更新豁免乐观锁：仅回写管理员用户 Id。
            await Repo.AsUpdateable(tenant).UpdateColumns(item => new { item.AdminUserId }).ExecuteCommandAsync(ct);
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(long id, SaveTenantInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("租户不存在");
        await EnsureCodeUniqueAsync(input.Code, id, ct);
        ValidateDatabaseConfig(input);

        var typeChangedToDb = entity.Type != TenantTypeEnum.Db && input.Type == TenantTypeEnum.Db;
        var dbConfigChanged = !string.Equals(entity.DbConfigId, input.DbConfigId, StringComparison.OrdinalIgnoreCase);

        entity.Name = input.Name;
        entity.Code = input.Code;
        entity.Type = input.Type;
        entity.DbConfigId = input.DbConfigId;
        entity.AdminUserId = input.AdminUserId;
        entity.ExpireTime = input.ExpireTime;
        entity.OrderNo = input.OrderNo;
        entity.Status = input.Status;
        entity.Remark = input.Remark;
        entity.ModifiedBy = CurrentAccount.Id;
        entity.Modifier = CurrentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;

        if (input.Type != TenantTypeEnum.Db)
        {
            entity.ProvisionStatus = TenantProvisionStatusEnum.Ready;
            entity.ProvisionMessage = null;
        }

        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);

        if (input.Type == TenantTypeEnum.Db && (typeChangedToDb || dbConfigChanged
            || entity.ProvisionStatus != TenantProvisionStatusEnum.Ready))
        {
            entity.ProvisionStatus = TenantProvisionStatusEnum.Pending;
            entity.ProvisionMessage = null;
            // 列更新豁免乐观锁：开通流程中间态回写。
            await Repo.AsUpdateable(entity)
                .UpdateColumns(item => new { item.ProvisionStatus, item.ProvisionMessage })
                .ExecuteCommandAsync(ct);

            var provision = await databaseProvisioner.ProvisionAsync(entity, ct);
            entity.ProvisionStatus = provision.Succeeded
                ? TenantProvisionStatusEnum.Ready
                : TenantProvisionStatusEnum.Failed;
            entity.ProvisionMessage = provision.Message;
            await Repo.AsUpdateable(entity)
                .UpdateColumns(item => new { item.ProvisionStatus, item.ProvisionMessage })
                .ExecuteCommandAsync(ct);

            if (!provision.Succeeded)
                throw new BusinessException($"独立库开通失败：{provision.Message}");
        }
    }

    /// <inheritdoc />
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var hasUsers = await Db.Queryable<SysUser>().AnyAsync(user => user.TenantId == id, ct);
        if (hasUsers) throw new BusinessException("租户下仍有用户，不能删除");

        await Db.Deleteable<SysTenantMenu>().Where(item => item.TenantId == id).ExecuteCommandAsync(ct);
        var affected = await Db.Deleteable<SysTenant>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("租户不存在");
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<long>> GetMenuIdsAsync(long tenantId, CancellationToken ct = default)
    {
        await EnsureTenantExistsAsync(tenantId, ct);
        return await Db.Queryable<SysTenantMenu>()
            .Where(item => item.TenantId == tenantId)
            .Select(item => item.MenuId)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task GrantMenusAsync(long tenantId, IReadOnlyCollection<long> menuIds, CancellationToken ct = default)
    {
        await EnsureTenantExistsAsync(tenantId, ct);

        var distinctIds = menuIds.Where(menuId => menuId > 0).Distinct().ToArray();
        if (distinctIds.Length > 0)
        {
            var menus = await Db.Queryable<SysMenu>()
                .Where(menu => distinctIds.Contains(menu.Id))
                .Select(menu => new { menu.Id, menu.IsPlatform })
                .ToListAsync(ct);
            if (menus.Count != distinctIds.Length)
                throw new BusinessException("授权列表包含不存在的菜单或按钮");
            if (menus.Any(menu => menu.IsPlatform))
                throw new BusinessException("不能为租户启用平台专属菜单");
        }

        await ReplaceTenantMenusAsync(tenantId, distinctIds, ct);
        // 租户启用菜单 ∩ 角色菜单，启用集变更会影响全租户用户有效权限。
        await permissionCache.InvalidateByTenantAsync(tenantId, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MenuDto>> ListAssignableMenusAsync(CancellationToken ct = default)
        => await Db.Queryable<SysMenu>()
            .Where(item => !item.IsPlatform)
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
    /// 为 Id 隔离租户创建根机构、岗位、管理员用户与默认角色，并授予当前启用菜单。
    /// </summary>
    private async Task BootstrapTenantAdminAsync(
        SysTenant tenant,
        IReadOnlyCollection<long> menuIds,
        CancellationToken ct)
    {
        var account = $"{tenant.Code}_admin";
        if (account.Length > 32)
            account = account[..32];

        var accountExists = await Db.Queryable<SysUser>().AnyAsync(user => user.Account == account, ct);
        if (accountExists)
            throw new BusinessException($"默认管理员账号 '{account}' 已存在，请手动指定管理员或更换租户编码");

        var orgId = YitIdHelper.NextId();
        var posId = YitIdHelper.NextId();
        var userId = YitIdHelper.NextId();
        var roleId = YitIdHelper.NextId();
        var now = DateTime.Now;

        await Db.Insertable(new SysOrg
        {
            Id = orgId,
            TenantId = tenant.Id,
            ParentId = 0,
            Name = tenant.Name,
            Code = tenant.Code,
            Type = OrgTypeEnum.Company,
            OrderNo = 1,
            Status = StatusEnum.Enable,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = now
        }).ExecuteCommandAsync(ct);

        await Db.Insertable(new SysPos
        {
            Id = posId,
            TenantId = tenant.Id,
            Name = "租户管理员",
            Code = "tenant-admin",
            OrderNo = 1,
            Status = StatusEnum.Enable,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = now
        }).ExecuteCommandAsync(ct);

        await Db.Insertable(new SysUser
        {
            Id = userId,
            TenantId = tenant.Id,
            OrgId = orgId,
            PosId = posId,
            Account = account,
            RealName = $"{tenant.Name}管理员",
            NickName = "管理员",
            Password = BCrypt.Net.BCrypt.HashPassword(UserAccountDefaults.InitialPassword),
            AccountType = AccountTypeEnum.SysAdmin,
            OrderNo = 1,
            Status = StatusEnum.Enable,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = now
        }).ExecuteCommandAsync(ct);

        await Db.Insertable(new SysRole
        {
            Id = roleId,
            TenantId = tenant.Id,
            Name = "租户管理员",
            Code = TenantAdminRoleCode,
            DataScope = DataScopeEnum.All,
            OrderNo = 1,
            Status = StatusEnum.Enable,
            Remark = "租户开通时自动创建，拥有租户启用菜单的全部权限",
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = now
        }).ExecuteCommandAsync(ct);

        if (menuIds.Count > 0)
        {
            var roleMenus = menuIds.Select(menuId => new SysRoleMenu
            {
                Id = YitIdHelper.NextId(),
                RoleId = roleId,
                MenuId = menuId
            }).ToArray();
            await Db.Insertable(roleMenus).ExecuteCommandAsync(ct);
        }

        await Db.Insertable(new SysUserRole
        {
            Id = YitIdHelper.NextId(),
            UserId = userId,
            RoleId = roleId
        }).ExecuteCommandAsync(ct);

        tenant.AdminUserId = userId;
        // 列更新豁免乐观锁：开通管理员后回写 AdminUserId。
        await Repo.AsUpdateable(tenant).UpdateColumns(item => new { item.AdminUserId }).ExecuteCommandAsync(ct);
    }

    private async Task ReplaceTenantMenusAsync(long tenantId, IReadOnlyCollection<long> menuIds, CancellationToken ct)
    {
        await Db.Deleteable<SysTenantMenu>().Where(item => item.TenantId == tenantId).ExecuteCommandAsync(ct);
        if (menuIds.Count == 0) return;

        var relations = menuIds.Select(menuId => new SysTenantMenu
        {
            Id = YitIdHelper.NextId(),
            TenantId = tenantId,
            MenuId = menuId
        }).ToArray();
        await Db.Insertable(relations).ExecuteCommandAsync(ct);
    }

    private async Task<long[]> LoadDefaultAssignableMenuIdsAsync(CancellationToken ct)
        => (await Db.Queryable<SysMenu>()
            .Where(menu => !menu.IsPlatform && menu.Status == StatusEnum.Enable)
            .Select(menu => menu.Id)
            .ToListAsync(ct)).ToArray();

    private async Task EnsureTenantExistsAsync(long tenantId, CancellationToken ct)
    {
        var exists = await Repo.AsQueryable().AnyAsync(item => item.Id == tenantId, ct);
        if (!exists) throw new BusinessException("租户不存在");
    }

    private async Task EnsureCodeUniqueAsync(string code, long? excludedId, CancellationToken ct)
    {
        var exists = await Repo.AsQueryable()
            .Where(item => item.Code == code)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"租户编码 '{code}' 已存在");
    }

    /// <summary>
    /// 独立库须填写且属于启动连接配置；禁止使用主库 ConfigId。
    /// </summary>
    private static void ValidateDatabaseConfig(SaveTenantInput input)
    {
        if (input.Type != TenantTypeEnum.Db) return;

        if (string.IsNullOrWhiteSpace(input.DbConfigId))
            throw new BusinessException("独立数据库租户必须填写数据库配置编号");

        if (string.Equals(input.DbConfigId, SqlSugarConst.MainConfigId, StringComparison.OrdinalIgnoreCase))
            throw new BusinessException("独立库不能使用主库配置编号 master");

        var known = GlobalContext.BootstrapDb?.ConnectionConfigs?
            .Any(config => string.Equals(
                config.ConfigId?.ToString(),
                input.DbConfigId,
                StringComparison.OrdinalIgnoreCase)) == true;

        if (!known)
            throw new BusinessException($"数据库配置编号 '{input.DbConfigId}' 不存在或未加载");
    }
}
