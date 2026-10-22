using System.Security.Claims;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Author.Internal;
using Axis.Infrastructure.Components.Caching.Services;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Constants;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Security;
using Axis.Infrastructure.Security.OpenApi;

namespace Axis.Application.System.OpenApiIdentity;

/// <summary>
/// OpenAPI 签名验签：查身份、验 HMAC、登记 Nonce、构造绑定用户 Claims。
/// </summary>
public sealed class OpenApiSignatureAuthService(
    ISysOpenApiIdentityRepository identityRepo,
    IRepository<SysUser> userRepo,
    IRepository<SysOrg> orgRepo,
    ICacheService cache)
    : IOpenApiSignatureAuthService, IAutoInject
{
    /// <inheritdoc />
    public async Task<OpenApiSignatureAuthResult> AuthenticateAsync(
        string accessKey,
        string timestamp,
        string nonce,
        string sign,
        string method,
        string path,
        SignatureAuthenticationOptions options,
        CancellationToken ct = default)
    {
        if (nonce.Length < Math.Clamp(options.MinNonceLength, 4, 64))
            return OpenApiSignatureAuthResult.Fail("nonce 无效");

        if (!long.TryParse(timestamp, out var unixSeconds))
            return OpenApiSignatureAuthResult.Fail("timestamp 无效");

        var clientTime = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        var skew = TimeSpan.FromSeconds(Math.Clamp(options.MaxSkewSeconds, 30, 3600));
        var now = DateTimeOffset.UtcNow;
        if (clientTime < now - skew || clientTime > now + skew)
            return OpenApiSignatureAuthResult.Fail("请求已过期或客户端时钟偏差过大");

        var identity = await identityRepo.FindEnabledByAccessKeyAsync(accessKey, ct);
        if (identity is null)
            return OpenApiSignatureAuthResult.Fail("开放接口身份无效");

        var payload = RequestHmacSigner.BuildOpenApiPayload(method, path, accessKey, timestamp, nonce);
        var expected = RequestHmacSigner.SignBase64(identity.AccessSecret, payload);
        if (!RequestHmacSigner.FixedEquals(expected, sign))
            return OpenApiSignatureAuthResult.Fail("签名无效");

        var cacheKey = $"{CacheCatalog.OpenApiNonce.KeyPrefix}{accessKey}:{nonce}";
        // 原子占坑，避免 Exists+Set 并发双放行。
        if (!await cache.TrySetAsync(cacheKey, 1, skew, ct))
            return OpenApiSignatureAuthResult.Fail("检测到重复请求，请勿重放");

        var user = await userRepo.AsQueryable()
            .ClearFilter()
            .Where(item => item.Id == identity.UserId && !item.Deleted)
            .FirstAsync(ct);
        if (user is null || user.Status == StatusEnum.Disable)
            return OpenApiSignatureAuthResult.Fail("绑定用户不存在或已停用");

        var tenantId = identity.TenantId ?? user.TenantId;
        var orgName = string.Empty;
        if (user.OrgId > 0)
        {
            var org = await orgRepo.AsQueryable()
                .ClearFilter()
                .Where(item => item.Id == user.OrgId && !item.Deleted)
                .FirstAsync(ct);
            orgName = org?.Name ?? string.Empty;
        }

        var claims = new List<Claim>
        {
            new(ClaimConst.AccountId, user.Id.ToString()),
            new(ClaimConst.AccountName, user.Account),
            new(ClaimConst.RealName, user.RealName ?? string.Empty),
            new(ClaimConst.AccountType, ((int)user.AccountType).ToString()),
            new(ClaimConst.OrgId, user.OrgId.ToString()),
            new(ClaimConst.OrgName, orgName),
            new(ClaimConst.TenantId, tenantId?.ToString() ?? string.Empty),
            new("ak", accessKey)
        };

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(claims, SignatureAuthenticationDefaults.AuthenticationScheme));
        return OpenApiSignatureAuthResult.Ok(principal);
    }
}
