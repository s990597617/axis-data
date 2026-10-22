using Axis.Application.Common;
using Axis.Application.System.Online;
using Axis.Application.System.User.Dto;
using Axis.Domain.Enums;
using Axis.Domain.Data.System.Cache;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Excel;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using MapsterMapper;
using SqlSugar;
using Yitter.IdGenerator;

namespace Axis.Application.System.User;

/// <summary>
/// 用户管理应用服务。
/// </summary>
/// <remarks>
/// 负责用户主表维护；多表联查下沉到 <see cref="ISysUserRepository"/>，实体到 DTO 走 Mapster；机构树与登录会话不在此模块。
/// 数据范围由 SqlSugar 全局 QueryFilter 自动附加，业务层无需手写过滤。
/// </remarks>
public sealed partial class UserAppService(
    ISysUserRepository userRepo,
    ISqlSugarClient db,
    ICurrentAccount currentAccount,
    IOnlineSessionAppService onlineSession,
    IUserPermissionCache permissionCache,
    IMapper mapper,
    IExcelService excel)
    : BaseAppService<SysUser>(userRepo, db, currentAccount), IUserAppService, IAutoInject
{
    /// <summary>
    /// 按条件分页查询用户列表，机构名称由仓储携带的组织导航填充。
    /// </summary>
    /// <param name="query">分页与筛选条件；关键字匹配账号、姓名、昵称、手机号。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>用户 DTO 分页结果。</returns>
    public async Task<SqlSugarPagedList<UserDto>> PageAsync(UserPageQuery query, CancellationToken ct = default)
    {
        var paged = await userRepo.PageWithOrgAsync(
            query.Page, query.PageSize, query.OrgId, query.Keyword, query.Status, ct);

        return new SqlSugarPagedList<UserDto>
        {
            Page = paged.Page,
            PageSize = paged.PageSize,
            Total = paged.Total,
            TotalPages = paged.TotalPages,
            HasPrevPage = paged.HasPrevPage,
            HasNextPage = paged.HasNextPage,
            Items = mapper.Map<List<UserDto>>(paged.Items.ToList())
        };
    }

    /// <summary>
    /// 按主键获取用户详情；不存在或不在数据权限内时返回 null。
    /// </summary>
    /// <param name="id">用户主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>用户 DTO；不存在或不在数据权限内时为 null。</returns>
    public new async Task<UserDto?> GetAsync(long id, CancellationToken ct = default)
    {
        var user = await userRepo.FindByIdWithOrgAsync(id, ignoreFilter: false, ct);
        if (user is null) return null;

        var dto = mapper.Map<UserDto>(user);
        var roles = await Db.Queryable<SysUserRole, SysRole>((ur, role) => ur.RoleId == role.Id)
            .Where((ur, role) => ur.UserId == id)
            .OrderBy((ur, role) => role.OrderNo)
            .Select((ur, role) => new { role.Id, role.Name })
            .ToListAsync(ct);
        dto.RoleIds = roles.Select(item => item.Id).ToList();
        dto.RoleNames = roles.Select(item => item.Name).ToList();
        return dto;
    }

    /// <summary>
    /// 校验账号唯一后创建用户；初始密码使用系统默认值并 BCrypt 哈希，同时覆盖保存角色关联。
    /// </summary>
    /// <param name="input">创建用户字段（不含密码）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">账号已存在、机构越权或角色 Id 无效时抛出。</exception>
    public async Task CreateAsync(CreateUserInput input, CancellationToken ct = default)
    {
        await EnsureOrgAccessibleAsync(input.OrgId, ct);

        var exists = await Repo.AsQueryable().AnyAsync(u => u.Account == input.Account, ct);
        if (exists) throw new BusinessException($"账号 '{input.Account}' 已存在");

        var user = mapper.Map<SysUser>(input);
        user.TenantId = CurrentAccount.TenantId;
        user.Password = BCrypt.Net.BCrypt.HashPassword(UserAccountDefaults.InitialPassword);

        await Db.Insertable(user).ExecuteCommandAsync(ct);
        await ReplaceUserRolesAsync(user.Id, input.RoleIds, ct);
    }

    /// <summary>
    /// 更新用户资料（不含账号与密码），并覆盖保存角色关联。
    /// </summary>
    /// <param name="input">更新字段；须包含有效用户 Id。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">用户不存在、无权访问、机构越权或角色 Id 无效时抛出。</exception>
    public async Task UpdateAsync(UpdateUserInput input, CancellationToken ct = default)
    {
        var user = await Repo.AsQueryable().Where(u => u.Id == input.Id).FirstAsync(ct)
                   ?? throw new BusinessException("用户不存在");

        await EnsureOrgAccessibleAsync(input.OrgId, ct);

        mapper.Map(input, user);
        // 以客户端打开详情时的版本作为乐观锁比较值（SqlSugar 成功后自动推进）。
        user.Version = input.Version;

        await Db.Updateable(user)
            .IgnoreColumns(u => new { u.Account, u.Password, u.TenantId, u.CreatedBy, u.Creator, u.CreatedTime })
            .ExecuteWithOptLockAsync(ct);
        await ReplaceUserRolesAsync(user.Id, input.RoleIds, ct);
    }

    /// <summary>
    /// 删除指定用户及其角色关联。
    /// </summary>
    /// <param name="id">用户主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">用户不存在或无权操作时抛出。</exception>
    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var exists = await Repo.AsQueryable().AnyAsync(u => u.Id == id, ct);
        if (!exists) throw new BusinessException("用户不存在");

        await Db.Deleteable<SysUserRole>().Where(item => item.UserId == id).ExecuteCommandAsync(ct);
        await Db.Deleteable<SysUser>().Where(u => u.Id == id).ExecuteCommandAsync(ct);
        await permissionCache.InvalidateAsync(id, ct);
    }

    /// <summary>
    /// 重置用户密码并以 BCrypt 哈希后落库。
    /// </summary>
    /// <param name="id">用户主键。</param>
    /// <param name="newPassword">新密码明文。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">用户不存在时抛出。</exception>
    public async Task ResetPasswordAsync(long id, string newPassword, CancellationToken ct = default)
    {
        var exists = await Repo.AsQueryable().AnyAsync(u => u.Id == id, ct);
        if (!exists) throw new BusinessException("用户不存在");

        // 列更新豁免乐观锁：仅改密码，不携带 Version。
        await Repo.AsUpdateable()
            .SetColumns(u => new SysUser { Password = BCrypt.Net.BCrypt.HashPassword(newPassword) })
            .Where(u => u.Id == id)
            .ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 修改用户启用状态。
    /// </summary>
    /// <param name="id">用户主键。</param>
    /// <param name="status">目标状态。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">用户不存在或无权操作时抛出。</exception>
    public async Task ChangeStatusAsync(long id, StatusEnum status, CancellationToken ct = default)
    {
        var exists = await Repo.AsQueryable().AnyAsync(u => u.Id == id, ct);
        if (!exists) throw new BusinessException("用户不存在");

        // 列更新豁免乐观锁：启停开关冲突面小。
        await Repo.AsUpdateable()
            .SetColumns(u => new SysUser { Status = status })
            .Where(u => u.Id == id)
            .ExecuteCommandAsync(ct);

        if (status == StatusEnum.Disable)
            await onlineSession.ForceLogoutUserAsync(id, "账号已被冻结，请联系管理员", checkPermission: false, ct: ct);
    }

    /// <summary>
    /// 通过全局机构过滤器校验目标机构是否可访问。
    /// </summary>
    private async Task EnsureOrgAccessibleAsync(long orgId, CancellationToken ct)
    {
        var exists = await Db.Queryable<SysOrg>().AnyAsync(item => item.Id == orgId, ct);
        if (!exists) throw new BusinessException("无权操作该机构下的用户");
    }

    /// <summary>
    /// 覆盖保存用户角色关联；先校验角色存在，再删旧插新。
    /// </summary>
    private async Task ReplaceUserRolesAsync(long userId, IReadOnlyCollection<long> roleIds, CancellationToken ct)
    {
        var distinctIds = roleIds.Where(id => id > 0).Distinct().ToArray();
        if (distinctIds.Length > 0)
        {
            var existingCount = await Db.Queryable<SysRole>()
                .CountAsync(role => distinctIds.Contains(role.Id), ct);
            if (existingCount != distinctIds.Length)
                throw new BusinessException("角色列表包含不存在的角色");
        }

        await Db.Deleteable<SysUserRole>().Where(item => item.UserId == userId).ExecuteCommandAsync(ct);
        if (distinctIds.Length == 0)
        {
            await permissionCache.InvalidateAsync(userId, ct);
            return;
        }

        var relations = distinctIds.Select(roleId => new SysUserRole
        {
            Id = YitIdHelper.NextId(),
            UserId = userId,
            RoleId = roleId
        }).ToArray();
        await Db.Insertable(relations).ExecuteCommandAsync(ct);
        // 角色变更后旧权限码立即失效，下次鉴权或登录再回源。
        await permissionCache.InvalidateAsync(userId, ct);
    }
}
