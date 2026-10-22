using Axis.Application.Author.Dto;
using Axis.Application.Author.Internal;
using Axis.Domain.Enums;
using Axis.Domain.Data.System.Cache;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Author.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Host.Responses;

namespace Axis.Application.Author;

/// <summary>
/// 登录后用户资料与菜单装配服务。
/// </summary>
/// <remarks>
/// 不签发或轮换令牌；会话能力见 <see cref="IAuthorAppService"/>。
/// </remarks>
public sealed class AuthorProfileAppService(
    ISysUserRepository userRepo,
    ISysMenuRepository menuRepo,
    ISysRoleRepository roleRepo,
    IRepository<SysOrg> orgRepo,
    IRepository<SysPos> posRepo,
    IUserPermissionCache permissionCache,
    ICurrentAccount currentAccount)
    : IAuthorProfileAppService, IAutoInject
{
    /// <summary>
    /// 装配当前登录用户的完整资料（机构、角色、按钮权限、菜单树）。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>供前端初始化路由与权限的用户信息。</returns>
    /// <exception cref="BusinessException">未登录或用户不存在时抛出未授权错误。</exception>
    public async Task<LoginUserInfo> GetUserInfoAsync(CancellationToken ct = default)
    {
        if (currentAccount.Id == 0)
            throw new BusinessException(ResultCodes.Unauthorized, "非法操作，未登录!");

        var user = await userRepo.FindByIdWithOrgAsync(currentAccount.Id, ignoreFilter: false, ct)
                   ?? throw new BusinessException(ResultCodes.Unauthorized, "非法操作，未登录!");

        var org = await LoadOrgAsync(user.OrgId, ct);
        var pos = await LoadPosAsync(user.PosId, ct);
        var isSuperAdmin = user.AccountType == AccountTypeEnum.SuperAdmin;
        var roleIds = await userRepo.GetRoleIdsAsync(user.Id, ct);
        var roles = await roleRepo.GetEnabledCodesAsync(roleIds, ct);
        // 与端点鉴权同源：优先读权限缓存（登录已预热；未命中则回源回填）。
        var buttons = await permissionCache.GetAsync(user.Id, isSuperAdmin, ct);
        var menus = await menuRepo.GetUserAllMenusAsync(user.Id, isSuperAdmin, ct);

        return new LoginUserInfo
        {
            Id = user.Id,
            Account = user.Account,
            RealName = user.RealName,
            Phone = user.Phone,
            AccountType = user.AccountType,
            TenantId = user.TenantId,
            Avatar = user.Avatar,
            OrgId = user.OrgId,
            OrgName = org?.Name,
            OrgType = org?.Type.ToString(),
            PosName = pos?.Name,
            Roles = roles,
            Buttons = buttons,
            MenuTrees = LoginMenuTreeBuilder.Build(menus)
        };
    }

    /// <summary>
    /// 按 OrgId 加载机构；允许缺失，避免脏数据导致资料接口失败。
    /// </summary>
    /// <param name="orgId">机构主键；0 表示无机构。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>机构实体；不存在时为 null。</returns>
    private async Task<SysOrg?> LoadOrgAsync(long orgId, CancellationToken ct)
    {
        if (orgId == 0) return null;
        return await orgRepo.GetByIdAsync(orgId, ct);
    }

    /// <summary>
    /// 按 PosId 加载岗位；允许缺失。
    /// </summary>
    private async Task<SysPos?> LoadPosAsync(long posId, CancellationToken ct)
    {
        if (posId == 0) return null;
        return await posRepo.GetByIdAsync(posId, ct);
    }
}
