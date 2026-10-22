using System.Data;
using System.Security.Cryptography;
using System.Text;
using Axis.Application.Author.Dto;
using Axis.Domain.Enums;
using Axis.Domain.Data.System.Cache;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.System;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Author.Internal;
using Axis.Infrastructure.Components.Caching.Services;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Components.Database.UnitOfWork;
using Axis.Infrastructure.Components.EventBus;
using Axis.Infrastructure.Components.EventBus.Events;
using Axis.Infrastructure.Constants;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Global;
using Axis.Infrastructure.Host.Responses;
using Axis.Infrastructure.Notify;
using Axis.Infrastructure.Security;
using FastEndpoints.Security;
using Lazy.Captcha.Core;
using Microsoft.AspNetCore.Http;
using Yitter.IdGenerator;

namespace Axis.Application.Author;

/// <summary>
/// 登录会话应用服务。
/// </summary>
/// <remarks>
/// 负责验证码、登录、刷新令牌轮换与退出；用户资料与菜单见 <see cref="IAuthorProfileAppService"/>。
/// </remarks>
public sealed class AuthorAppService(
    ISysUserRepository userRepo,
    ISysRefreshTokenRepository refreshTokenRepo,
    IRepository<SysOrg> orgRepo,
    IRepository<SysTenant> tenantRepo,
    ICacheService cacheService,
    IUserPermissionCache permissionCache,
    IAccessTokenDenyService accessTokenDeny,
    ISessionKickPusher sessionKickPusher,
    ICaptcha captcha,
    IUnitOfWork unitOfWork,
    IHttpContextAccessor httpContextAccessor,
    IEventPublisher eventPublisher)
    : IAuthorAppService, IAutoInject
{
    /// <summary>
    /// Web 端登录：校验锁定与验证码，解密并校验密码后签发令牌对。
    /// </summary>
    /// <param name="req">账号、SM2 密文密码、验证码与记住我选项。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>AccessToken 与 RefreshToken 配对；RefreshToken 明文仅此返回一次。</returns>
    /// <exception cref="BusinessException">验证码错误、账号锁定、密码错误或账号冻结时抛出。</exception>
    public async Task<IssuedTokenPair> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        await EnsureNotLockedAsync(req.Account, ct);

        if (!captcha.Validate(req.CodeId, req.Code))
            throw new BusinessException("验证码错误");

        var user = await LoadUserForLoginAsync(req.Account, ct);
        await VerifyPasswordAsync(user, req.Password, ct);
        await EnsureTenantAllowsLoginAsync(user, ct);

        // 直接切换为哈希存储，不再兼容旧版明文 Refresh Token 会话。
        await refreshTokenRepo.DeleteLegacyAsync(ct);

        var result = await IssueTokensAsync(user, req.RememberMe, ct: ct);
        // 登录成功即预热权限码缓存，避免首个业务接口鉴权再查库。
        await permissionCache.RefreshAsync(
            user.Id,
            user.AccountType == AccountTypeEnum.SuperAdmin,
            ct);
        await PublishLoginEventAsync(user, ct);
        return result;
    }

    /// <summary>
    /// 使用刷新令牌哈希查找未吊销会话，条件消费成功后签发新的令牌对。
    /// </summary>
    /// <param name="refreshToken">客户端持有的明文刷新令牌；服务端仅存储其哈希。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>新的 AccessToken 与 RefreshToken 配对。</returns>
    /// <exception cref="BusinessException">令牌无效、已使用、已过期或账号冻结时抛出。</exception>
    public async Task<IssuedTokenPair> RefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw SessionExpired();

        var tokenHash = HashRefreshToken(refreshToken);
        var model = await refreshTokenRepo.FindByHashAsync(tokenHash, ct)
                    ?? throw SessionExpired();
        var now = DateTime.Now;

        // 已消费 RT 再次出现：判定为重放或泄漏，整族吊销（含当前合法会话）。
        // 注意：TryConsume 竞态失败不走此分支，避免多标签并发刷新误杀整族。
        if (model.IsUsed)
        {
            await HandleRefreshTokenReuseAsync(model, now, ct);
            throw SessionExpired();
        }

        if (model.RevokedTime.HasValue || model.ExpiryDate <= now)
            throw SessionExpired();

        unitOfWork.Begin(IsolationLevel.ReadCommitted);
        try
        {
            // 条件更新是轮换的并发闸门：同一 Refresh Token 只能有一个请求成功。
            var affected = await refreshTokenRepo.TryConsumeAsync(model.Id, now, ct);
            if (affected != 1)
                throw SessionExpired();

            // 轮换成功后吊销父会话配对的 AccessToken，避免旧 AT 继续访问。
            if (!string.IsNullOrWhiteSpace(model.AccessTokenJti))
            {
                var expiresAt = model.AccessTokenExpiresAt
                                ?? now.AddSeconds(GlobalContext.BootstrapJwt.TokenExpire);
                await accessTokenDeny.DenyAsync(model.AccessTokenJti, expiresAt, ct);
            }

            var user = await userRepo.FindByIdWithOrgAsync(model.UserId, ignoreFilter: true, ct)
                       ?? throw SessionExpired();

            if (user.Status == StatusEnum.Disable)
                throw new BusinessException(ResultCodes.Forbidden, "账号已被冻结，请联系管理员！");

            var result = await IssueTokensAsync(
                user,
                model.RememberMe == true,
                familyId: model.FamilyId,
                parentTokenId: model.Id,
                ct: ct);

            // 刷新会话时顺带刷新权限缓存，覆盖授权变更后的脏数据窗口。
            await permissionCache.RefreshAsync(
                user.Id,
                user.AccountType == AccountTypeEnum.SuperAdmin,
                ct);

            await unitOfWork.CommitAsync(ct);
            return result;
        }
        catch
        {
            await unitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// 按哈希吊销刷新令牌，并吊销配对 AccessToken；空令牌视为幂等成功。
    /// </summary>
    /// <param name="refreshToken">客户端持有的明文刷新令牌。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken)) return;

        var tokenHash = HashRefreshToken(refreshToken);
        var model = await refreshTokenRepo.FindByHashAsync(tokenHash, ct);
        await refreshTokenRepo.RevokeByHashAsync(tokenHash, DateTime.Now, ct);

        if (model is null || string.IsNullOrWhiteSpace(model.AccessTokenJti)) return;

        var expiresAt = model.AccessTokenExpiresAt
                        ?? DateTime.Now.AddSeconds(GlobalContext.BootstrapJwt.TokenExpire);
        await accessTokenDeny.DenyAsync(model.AccessTokenJti, expiresAt, ct);
    }

    /// <summary>
    /// 生成图形验证码及其 Id。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    /// <returns>验证码 Id 与 Base64 图片。</returns>
    public Task<CaptchaResponse> GetCaptchaAsync(CancellationToken ct = default)
    {
        var codeId = YitIdHelper.NextId().ToString();
        var data = captcha.Generate(codeId);
        return Task.FromResult(new CaptchaResponse
        {
            Id = codeId,
            Img = data.Base64
        });
    }

    /// <summary>
    /// 检查密码错误次数是否达到锁定阈值。
    /// </summary>
    /// <param name="account">登录账号。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">达到锁定阈值时抛出。</exception>
    private async Task EnsureNotLockedAsync(string account, CancellationToken ct)
    {
        var key = BuildErrorCountKey(account);
        var count = await cacheService.GetAsync<int>(key, ct);
        if (count >= CacheCatalog.ErrorPasswordMaxCount)
            throw new BusinessException($"密码错误次数过多，账号已锁定，请 {CacheCatalog.ErrorPasswordLockMinutes} 分钟后重试！");
    }

    /// <summary>
    /// 载入登录用户并做存在 / 状态校验。
    /// </summary>
    /// <param name="account">登录账号。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>可用于签发令牌的用户实体。</returns>
    /// <exception cref="BusinessException">账号不存在或已冻结时抛出。</exception>
    private async Task<SysUser> LoadUserForLoginAsync(string account, CancellationToken ct)
    {
        var user = await userRepo.FindByAccountWithOrgAsync(account, ct)
                   ?? throw new BusinessException("账号不存在");

        return user.Status == StatusEnum.Disable
            ? throw new BusinessException(ResultCodes.Forbidden, "账号已冻结")
            : user;
    }

    /// <summary>
    /// 校验用户所属租户允许登录；超级管理员跳过。登录不传租户，租户由账号归属决定。
    /// </summary>
    private async Task EnsureTenantAllowsLoginAsync(SysUser user, CancellationToken ct)
    {
        if (user.AccountType == AccountTypeEnum.SuperAdmin)
            return;

        if (!user.TenantId.HasValue)
            throw new BusinessException(ResultCodes.Forbidden, "账号未绑定租户，无法登录");

        var tenant = await tenantRepo.AsQueryable().Where(item => item.Id == user.TenantId.Value).FirstAsync(ct)
                     ?? throw new BusinessException(ResultCodes.Forbidden, "所属租户不存在");

        if (tenant.Status == StatusEnum.Disable)
            throw new BusinessException(ResultCodes.Forbidden, "所属租户已停用");

        if (tenant.ExpireTime.HasValue && tenant.ExpireTime.Value < DateTime.Now)
            throw new BusinessException(ResultCodes.Forbidden, "所属租户已过期");

        if (tenant.Type == TenantTypeEnum.Db && tenant.ProvisionStatus != TenantProvisionStatusEnum.Ready)
            throw new BusinessException(ResultCodes.Forbidden, "所属租户独立库尚未开通完成");
    }

    /// <summary>
    /// SM2 解密 + BCrypt 校验；失败时累加错误计数，成功时清空计数。
    /// </summary>
    /// <param name="user">待校验用户。</param>
    /// <param name="encryptedPassword">前端 SM2 加密后的密码密文。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">解密失败或密码错误时抛出统一错误，避免账号枚举。</exception>
    private async Task VerifyPasswordAsync(SysUser user, string encryptedPassword, CancellationToken ct)
    {
        var privateKey = GlobalContext.BootstrapSystem.PrivateKey;
        if (string.IsNullOrWhiteSpace(privateKey))
            throw new BusinessException(ResultCodes.ServerError, "服务端未配置 SM2 私钥，无法解密登录密码");

        string plain;
        try
        {
            plain = SmTools.Sm2Decrypt(privateKey, encryptedPassword);
        }
        catch
        {
            throw new BusinessException("账号或密码错误");
        }

        if (BCrypt.Net.BCrypt.Verify(plain, user.Password))
        {
            await cacheService.RemoveAsync(BuildErrorCountKey(user.Account), ct);
            return;
        }

        var key = BuildErrorCountKey(user.Account);
        var current = await cacheService.GetAsync<int>(key, ct);
        await cacheService.SetAsync(key, current + 1, TimeSpan.FromMinutes(CacheCatalog.ErrorPasswordLockMinutes), ct);
        throw new BusinessException("账号或密码错误");
    }

    /// <summary>
    /// 签发 AccessToken + RefreshToken 并落库。
    /// </summary>
    /// <param name="user">已通过校验的用户。</param>
    /// <param name="rememberMe">是否延长 Cookie 生命周期。</param>
    /// <param name="familyId">令牌族 Id；刷新时沿用，登录时新建。</param>
    /// <param name="parentTokenId">被轮换的父刷新令牌 Id；登录时为空。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>新签发的令牌对。</returns>
    private async Task<IssuedTokenPair> IssueTokensAsync(
        SysUser user,
        bool rememberMe,
        long? familyId = null,
        long? parentTokenId = null,
        CancellationToken ct = default)
    {
        var jwt = GlobalContext.BootstrapJwt;
        var now = DateTime.Now;
        var accessExpiry = now.AddSeconds(jwt.TokenExpire);
        var refreshExpiry = now.AddSeconds(jwt.RefreshTokenExpire);
        var org = await LoadOrgAsync(user.OrgId, ct);
        var accessTokenJti = YitIdHelper.NextId().ToString();
        var http = httpContextAccessor.HttpContext;

        var accessToken = JwtBearer.CreateToken(o =>
        {
            o.SigningKey = jwt.Token;
            o.Issuer = jwt.Issuer;
            o.Audience = jwt.Audience;
            o.ExpireAt = accessExpiry;
            o.User.Claims.Add((ClaimConst.JwtId, accessTokenJti));
            o.User.Claims.Add((ClaimConst.AccountId, user.Id.ToString()));
            o.User.Claims.Add((ClaimConst.TenantId, user.TenantId?.ToString() ?? string.Empty));
            o.User.Claims.Add((ClaimConst.AccountName, user.Account));
            o.User.Claims.Add((ClaimConst.RealName, user.RealName ?? string.Empty));
            o.User.Claims.Add((ClaimConst.AccountType, ((int)user.AccountType).ToString()));
            o.User.Claims.Add((ClaimConst.OrgId, user.OrgId.ToString()));
            o.User.Claims.Add((ClaimConst.OrgName, org?.Name ?? string.Empty));
        });

        var refreshToken = GenerateRefreshToken();
        var tokenId = YitIdHelper.NextId();
        var tokenFamilyId = familyId.GetValueOrDefault(tokenId);

        await refreshTokenRepo.InsertAsync(new SysRefreshToken
        {
            Id = tokenId,
            UserId = user.Id,
            TokenHash = HashRefreshToken(refreshToken),
            AccessTokenJti = accessTokenJti,
            AccessTokenExpiresAt = accessExpiry,
            ClientIp = http?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Truncate(http?.Request.Headers.UserAgent.ToString(), 512),
            FamilyId = tokenFamilyId,
            ParentTokenId = parentTokenId,
            IsUsed = false,
            ExpiryDate = refreshExpiry,
            RememberMe = rememberMe,
            CreatedTime = now,
            CreatedBy = user.Id,
            Creator = user.Account,
        }, ct);

        return new IssuedTokenPair(
            accessToken,
            new DateTimeOffset(accessExpiry).ToUnixTimeMilliseconds(),
            refreshToken,
            new DateTimeOffset(refreshExpiry),
            rememberMe);
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength) return value;
        return value[..maxLength];
    }

    /// <summary>
    /// 发布用户登录事件（Fire-and-Forget，不阻塞登录响应）。
    /// </summary>
    /// <param name="user">登录成功的用户。</param>
    /// <param name="ct">取消令牌。</param>
    private async Task PublishLoginEventAsync(SysUser user, CancellationToken ct)
    {
        var httpContext = httpContextAccessor.HttpContext;
        await eventPublisher.PublishFireAndForgetAsync(new UserLoginEvent
        {
            UserId = user.Id,
            Account = user.Account,
            RemoteIp = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString()
        }, ct);
    }

    private static string BuildErrorCountKey(string account) => $"{CacheCatalog.LoginError.KeyPrefix}{account}";

    /// <summary>
    /// 生成 256-bit 随机刷新令牌（Base64 URL safe）。
    /// </summary>
    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string HashRefreshToken(string refreshToken)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

    /// <summary>
    /// Refresh Token 复用：吊销同族全部未撤销成员、废掉配对 AT，并推送强制下线。
    /// </summary>
    private async Task HandleRefreshTokenReuseAsync(
        SysRefreshToken reused,
        DateTime now,
        CancellationToken ct)
    {
        var familyId = reused.FamilyId.GetValueOrDefault();
        IReadOnlyList<SysRefreshToken> revoked;
        if (familyId > 0)
        {
            revoked = await refreshTokenRepo.RevokeByFamilyIdAsync(familyId, now, ct);
        }
        else
        {
            // 无族信息的遗留行：至少吊销当前哈希，避免只报错不处理。
            await refreshTokenRepo.RevokeByHashAsync(reused.TokenHash, now, ct);
            revoked = [reused];
        }

        var denyItems = revoked
            .Where(t => !string.IsNullOrWhiteSpace(t.AccessTokenJti))
            .Select(t => (
                t.AccessTokenJti!,
                t.AccessTokenExpiresAt ?? now.AddSeconds(GlobalContext.BootstrapJwt.TokenExpire)))
            .ToArray();
        if (denyItems.Length > 0)
            await accessTokenDeny.DenyManyAsync(denyItems, ct);

        await sessionKickPusher.PushForceLogoutAsync(
            reused.UserId,
            new ForceLogoutPayload
            {
                Reason = "检测到登录凭证异常，请重新登录"
            },
            ct);
    }

    private static BusinessException SessionExpired()
        => new(ResultCodes.TokenExpired, "登录已过期，请重新登录！");

    /// <summary>
    /// 按 OrgId 加载机构；允许缺失，避免脏数据导致登录失败。
    /// </summary>
    /// <param name="orgId">机构主键；0 表示无机构。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>机构实体；不存在时为 null。</returns>
    private async Task<SysOrg?> LoadOrgAsync(long orgId, CancellationToken ct)
    {
        if (orgId == 0) return null;
        return await orgRepo.GetByIdAsync(orgId, ct);
    }
}
