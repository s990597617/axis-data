using Axis.Application.System.Online.Dto;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Author.Internal;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Global;
using Axis.Infrastructure.Notify;
using Axis.Infrastructure.Security;

namespace Axis.Application.System.Online;

/// <summary>
/// 在线会话管理：列表、按会话 / 按用户强制下线。
/// </summary>
public sealed class OnlineSessionAppService(
    ISysRefreshTokenRepository refreshTokenRepo,
    IRepository<SysUser> userRepo,
    IAccessTokenDenyService accessTokenDeny,
    ISessionKickPusher sessionKickPusher,
    ICurrentAccount currentAccount)
    : IOnlineSessionAppService, IAutoInject
{
    private const string DefaultReason = "您已被管理员强制下线，请重新登录";

    /// <summary>
    /// 分页查询仍有效的登录会话，并标记当前 AccessToken 对应行。
    /// </summary>
    /// <param name="query">分页与关键字（账号 / 姓名 / IP）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>在线会话 DTO 分页结果。</returns>
    public async Task<SqlSugarPagedList<OnlineSessionDto>> PageAsync(
        OnlineSessionPageQuery query,
        CancellationToken ct = default)
    {
        var paged = await refreshTokenRepo.PageActiveSessionsAsync(
            query.Keyword, query.Page, query.PageSize, ct);
        var currentJti = currentAccount.JwtId;

        var items = paged.Items.Select(row => new OnlineSessionDto
        {
            Id = row.Id,
            UserId = row.UserId,
            Account = row.Account,
            RealName = row.RealName,
            AccountType = row.AccountType,
            ClientIp = row.ClientIp,
            UserAgent = row.UserAgent,
            CreatedTime = row.CreatedTime,
            ExpiryDate = row.ExpiryDate,
            RememberMe = row.RememberMe == true,
            IsCurrent = !string.IsNullOrEmpty(currentJti)
                        && string.Equals(row.AccessTokenJti, currentJti, StringComparison.Ordinal)
        }).ToList();

        return new SqlSugarPagedList<OnlineSessionDto>
        {
            Page = paged.Page,
            PageSize = paged.PageSize,
            Total = paged.Total,
            TotalPages = paged.TotalPages,
            HasPrevPage = paged.HasPrevPage,
            HasNextPage = paged.HasNextPage,
            Items = items
        };
    }

    /// <summary>
    /// 强制下线指定会话：吊销 RefreshToken、写入 AT 拒绝名单并推送 ForceLogout。
    /// </summary>
    /// <param name="sessionId">会话主键（RefreshToken Id）。</param>
    /// <param name="reason">下线原因文案。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task ForceLogoutSessionAsync(
        long sessionId,
        string? reason = null,
        CancellationToken ct = default)
    {
        var session = await refreshTokenRepo.GetByIdAsync(sessionId, ct)
                      ?? throw new BusinessException("会话不存在或已下线");

        await EnsureCanKickUserAsync(session.UserId, ct);

        var affected = await refreshTokenRepo.RevokeByIdAsync(sessionId, DateTime.Now, ct);
        if (affected == 0)
            throw new BusinessException("会话不存在或已下线");

        await DenyTokenAsync(session.AccessTokenJti, session.AccessTokenExpiresAt, ct);
        await PushKickAsync(session.UserId, sessionId.ToString(), reason, ct);
    }

    /// <summary>
    /// 强制下线用户全部活跃会话；可由用户冻结等内部场景关闭权限校验。
    /// </summary>
    /// <param name="userId">目标用户主键。</param>
    /// <param name="reason">下线原因文案。</param>
    /// <param name="checkPermission">是否校验「不可踢超管」。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task ForceLogoutUserAsync(
        long userId,
        string? reason = null,
        bool checkPermission = true,
        CancellationToken ct = default)
    {
        if (userId <= 0) throw new BusinessException("用户无效");
        if (checkPermission)
            await EnsureCanKickUserAsync(userId, ct);

        var revoked = await refreshTokenRepo.RevokeByUserIdAsync(userId, DateTime.Now, ct);
        foreach (var item in revoked)
            await DenyTokenAsync(item.AccessTokenJti, item.AccessTokenExpiresAt, ct);

        await PushKickAsync(userId, null, reason, ct);
    }

    /// <summary>
    /// 校验操作者是否可踢目标用户（禁止非超管踢超管）。
    /// </summary>
    private async Task EnsureCanKickUserAsync(long targetUserId, CancellationToken ct)
    {
        if (targetUserId == currentAccount.Id) return;

        var user = await userRepo.GetByIdAsync(targetUserId, ct);
        if (user is null) throw new BusinessException("用户不存在");

        if (user.AccountType == AccountTypeEnum.SuperAdmin && !currentAccount.IsSuperAdmin)
            throw new BusinessException("无权强制下线超级管理员");
    }

    /// <summary>
    /// 向目标用户推送强制下线事件。
    /// </summary>
    private Task PushKickAsync(long userId, string? sessionId, string? reason, CancellationToken ct)
        => sessionKickPusher.PushForceLogoutAsync(
            userId,
            new ForceLogoutPayload
            {
                Reason = string.IsNullOrWhiteSpace(reason) ? DefaultReason : reason!,
                SessionId = sessionId
            },
            ct);

    /// <summary>
    /// 将 AccessToken jti 写入拒绝名单。
    /// </summary>
    private async Task DenyTokenAsync(string? jti, DateTime? expiresAt, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(jti)) return;
        var until = expiresAt ?? DateTime.Now.AddSeconds(GlobalContext.BootstrapJwt.TokenExpire);
        await accessTokenDeny.DenyAsync(jti, until, ct);
    }
}
