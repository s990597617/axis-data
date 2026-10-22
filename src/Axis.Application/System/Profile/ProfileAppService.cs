using Axis.Application.System.Profile.Dto;
using Axis.Domain.Enums;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Global;
using Axis.Infrastructure.Host.Responses;
using Axis.Infrastructure.Security;
using SqlSugar;

namespace Axis.Application.System.Profile;

/// <summary>
/// 个人中心应用服务。
/// </summary>
/// <remarks>
/// 仅操作当前登录用户自身资料与密码，不开放改机构 / 岗位 / 角色。
/// </remarks>
public sealed class ProfileAppService(
    ISysUserRepository userRepo,
    ISysRoleRepository roleRepo,
    IRepository<SysOrg> orgRepo,
    IRepository<SysPos> posRepo,
    ICurrentAccount currentAccount)
    : IProfileAppService, IAutoInject
{
    /// <inheritdoc />
    public async Task<ProfileDto> GetAsync(CancellationToken ct = default)
    {
        var user = await RequireCurrentUserAsync(ct);
        var org = await LoadOrgAsync(user.OrgId, ct);
        var pos = await LoadPosAsync(user.PosId, ct);
        var roleIds = await userRepo.GetRoleIdsAsync(user.Id, ct);
        var roleNames = await LoadRoleNamesAsync(roleIds, ct);

        return new ProfileDto
        {
            Id = user.Id,
            Account = user.Account,
            RealName = user.RealName,
            NickName = user.NickName,
            Avatar = user.Avatar,
            Sex = user.Sex,
            Birthday = NormalizeBirthday(user.Birthday),
            Phone = user.Phone,
            Email = user.Email,
            Remark = user.Remark,
            JobNum = user.JobNum,
            JoinDate = user.JoinDate,
            AccountType = user.AccountType,
            OrgId = user.OrgId,
            OrgName = org?.Name,
            PosId = user.PosId,
            PosName = pos?.Name,
            RoleNames = roleNames,
            Status = user.Status,
            LastLoginIp = string.IsNullOrWhiteSpace(user.LastLoginIp) ? null : user.LastLoginIp,
            LastLoginTime = NormalizeDateTime(user.LastLoginTime),
            CreatedTime = user.CreatedTime
        };
    }

    /// <inheritdoc />
    public async Task UpdateAsync(UpdateProfileInput input, CancellationToken ct = default)
    {
        var user = await RequireCurrentUserAsync(ct);

        user.RealName = input.RealName.Trim();
        user.NickName = string.IsNullOrWhiteSpace(input.NickName) ? null! : input.NickName.Trim();
        user.Avatar = string.IsNullOrWhiteSpace(input.Avatar) ? null! : input.Avatar.Trim();
        user.Sex = input.Sex;
        user.Birthday = input.Birthday ?? default;
        user.Phone = string.IsNullOrWhiteSpace(input.Phone) ? null! : input.Phone.Trim();
        user.Email = string.IsNullOrWhiteSpace(input.Email) ? null! : input.Email.Trim();
        user.Remark = string.IsNullOrWhiteSpace(input.Remark) ? null! : input.Remark.Trim();

        // 列更新豁免乐观锁：个人中心局部字段更新，暂未回传 Version。
        await userRepo.AsUpdateable(user)
            .UpdateColumns(u => new
            {
                u.RealName,
                u.NickName,
                u.Avatar,
                u.Sex,
                u.Birthday,
                u.Phone,
                u.Email,
                u.Remark
            })
            .ExecuteCommandAsync(ct);
    }

    /// <inheritdoc />
    public async Task ChangePasswordAsync(ChangePasswordInput input, CancellationToken ct = default)
    {
        var user = await RequireCurrentUserAsync(ct);

        var oldPassword = DecryptPassword(input.OldPassword, "当前密码解密失败");
        var newPassword = DecryptPassword(input.NewPassword, "新密码解密失败");
        var confirmPassword = DecryptPassword(input.ConfirmPassword, "确认密码解密失败");

        if (newPassword.Length < 6)
            throw new BusinessException("新密码不能少于 6 个字符");
        if (newPassword.Length > 64)
            throw new BusinessException("新密码不能超过 64 个字符");
        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
            throw new BusinessException("两次输入的新密码不一致");
        if (string.Equals(oldPassword, newPassword, StringComparison.Ordinal))
            throw new BusinessException("新密码不能与当前密码相同");

        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
            throw new BusinessException("当前密码不正确");

        // 列更新豁免乐观锁：仅改密码哈希。
        await userRepo.AsUpdateable()
            .SetColumns(u => new SysUser { Password = BCrypt.Net.BCrypt.HashPassword(newPassword) })
            .Where(u => u.Id == user.Id)
            .ExecuteCommandAsync(ct);
    }

    /// <summary>
    /// 使用系统 SM2 私钥解密前端传来的密码密文。
    /// </summary>
    private static string DecryptPassword(string encryptedPassword, string failMessage)
    {
        var privateKey = GlobalContext.BootstrapSystem.PrivateKey;
        if (string.IsNullOrWhiteSpace(privateKey))
            throw new BusinessException(ResultCodes.ServerError, "服务端未配置 SM2 私钥，无法解密密码");

        try
        {
            var plain = SmTools.Sm2Decrypt(privateKey, encryptedPassword);
            if (string.IsNullOrEmpty(plain))
                throw new BusinessException(failMessage);
            return plain;
        }
        catch (BusinessException)
        {
            throw;
        }
        catch
        {
            throw new BusinessException(failMessage);
        }
    }

    private async Task<SysUser> RequireCurrentUserAsync(CancellationToken ct)
    {
        if (currentAccount.Id == 0)
            throw new BusinessException(ResultCodes.Unauthorized, "非法操作，未登录!");

        return await userRepo.FindByIdWithOrgAsync(currentAccount.Id, ignoreFilter: false, ct)
               ?? throw new BusinessException(ResultCodes.Unauthorized, "非法操作，未登录!");
    }

    private async Task<SysOrg?> LoadOrgAsync(long orgId, CancellationToken ct)
    {
        if (orgId == 0) return null;
        return await orgRepo.GetByIdAsync(orgId, ct);
    }

    private async Task<SysPos?> LoadPosAsync(long posId, CancellationToken ct)
    {
        if (posId == 0) return null;
        return await posRepo.GetByIdAsync(posId, ct);
    }

    private async Task<IReadOnlyList<string>> LoadRoleNamesAsync(
        IReadOnlyList<long> roleIds,
        CancellationToken ct)
    {
        if (roleIds.Count == 0) return Array.Empty<string>();

        var names = await roleRepo.AsQueryable()
            .Where(role => roleIds.Contains(role.Id) && role.Status == StatusEnum.Enable)
            .OrderBy(role => role.OrderNo)
            .Select(role => role.Name)
            .ToListAsync(ct);

        return names;
    }

    /// <summary>
    /// 实体 Birthday 为非空 DateTime，默认值视为未设置。
    /// </summary>
    private static DateTime? NormalizeBirthday(DateTime birthday)
        => birthday == default ? null : birthday;

    /// <summary>
    /// 非空 DateTime 默认值视为未设置。
    /// </summary>
    private static DateTime? NormalizeDateTime(DateTime value)
        => value == default ? null : value;
}
