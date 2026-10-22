using Axis.Infrastructure.Author.Internal;
using Axis.Infrastructure.Components.Caching.Services;
using Axis.Infrastructure.Components.FastEndpoints.Attributes;
using Axis.Infrastructure.Constants;
using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Host.Responses;
using Axis.Infrastructure.Options;
using Axis.Infrastructure.Security;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Axis.Infrastructure.Components.FastEndpoints.Processors;

/// <summary>
/// 请求级防重放前置处理器：时间戳窗口 + 一次性 Nonce + HMAC-SHA256。
/// </summary>
/// <remarks>
/// 仅对已认证的 POST/PUT/PATCH/DELETE 生效；GET/匿名、
/// <see cref="SkipAntiReplayAttribute"/> 与 <c>AntiReplay.Enabled=false</c> 时直接放行。
/// HMAC 密钥为当前 AccessToken；规范化串 <c>METHOD&amp;path&amp;timestamp&amp;nonce</c>。
/// Nonce 占坑使用 <c>TrySetAsync</c>（原子 SET NX），避免 Exists+Set 竞态双放行。
/// </remarks>
public sealed class AntiReplayPreProcessor : IGlobalPreProcessor
{
    private static readonly HashSet<string> MutatingMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Patch,
        HttpMethods.Delete
    };

    /// <summary>
    /// 校验签名、时间戳与 Nonce；不满足生效条件时直接返回。
    /// </summary>
    /// <param name="context">FastEndpoints 前置处理器上下文。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        var http = context.HttpContext;
        var options = http.RequestServices.GetService<IOptionsMonitor<AxisAuthOptions>>()?.CurrentValue.AntiReplay
                      ?? new AxisAntiReplayOptions();

        // 总开关关闭、读请求、未登录：不做防重放，避免登录/验证码/列表被误伤。
        if (!options.Enabled) return;
        if (!MutatingMethods.Contains(http.Request.Method)) return;
        if (http.User.Identity?.IsAuthenticated != true) return;

        // 对外 OpenAPI / Webhook 等：端点显式声明跳过，不要求 Axis 管理端防重放头。
        if (EndpointMetadataResolver.Resolve<SkipAntiReplayAttribute>(http) is not null)
            return;

        var skewSeconds = Math.Clamp(options.MaxSkewSeconds, 30, 3600);
        var minNonceLength = Math.Clamp(options.MinNonceLength, 8, 128);
        var timestampHeader = string.IsNullOrWhiteSpace(options.TimestampHeader)
            ? "X-Axis-Timestamp"
            : options.TimestampHeader;
        var nonceHeader = string.IsNullOrWhiteSpace(options.NonceHeader)
            ? "X-Axis-Nonce"
            : options.NonceHeader;
        var signHeader = string.IsNullOrWhiteSpace(options.SignHeader)
            ? "X-Axis-Sign"
            : options.SignHeader;

        var timestampRaw = http.Request.Headers[timestampHeader].ToString();
        var nonce = http.Request.Headers[nonceHeader].ToString().Trim();
        var sign = http.Request.Headers[signHeader].ToString().Trim();

        if (string.IsNullOrWhiteSpace(timestampRaw) || string.IsNullOrWhiteSpace(nonce))
        {
            throw new BusinessException(
                ResultCodes.BadRequest,
                $"缺少防重放请求头：{timestampHeader} / {nonceHeader}");
        }

        if (nonce.Length < minNonceLength)
        {
            throw new BusinessException(ResultCodes.BadRequest, "防重放 Nonce 无效");
        }

        if (!long.TryParse(timestampRaw, out var timestampMs))
        {
            throw new BusinessException(ResultCodes.BadRequest, "防重放时间戳无效");
        }

        var clientTime = DateTimeOffset.FromUnixTimeMilliseconds(timestampMs);
        var skew = TimeSpan.FromSeconds(skewSeconds);
        var now = DateTimeOffset.UtcNow;
        if (clientTime < now - skew || clientTime > now + skew)
        {
            throw new BusinessException(ResultCodes.BadRequest, "请求已过期或客户端时钟偏差过大");
        }

        if (options.RequireSignature)
        {
            var accessToken = ExtractBearerToken(http);
            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(sign))
            {
                throw new BusinessException(
                    ResultCodes.BadRequest,
                    $"缺少防重放签名请求头：{signHeader}");
            }

            // 绑定 method/path，防止把合法签名挪到其它写接口上重放。
            var path = http.Request.Path.Value ?? "/";
            var payload = RequestHmacSigner.BuildPayload(http.Request.Method, path, timestampRaw, nonce);
            var expected = RequestHmacSigner.SignBase64(accessToken, payload);
            if (!RequestHmacSigner.FixedEquals(expected, sign))
            {
                throw new BusinessException(ResultCodes.BadRequest, "请求签名无效");
            }
        }

        var cache = http.RequestServices.GetService<ICacheService>();
        if (cache is null)
        {
            var logger = http.RequestServices.GetService<ILoggerFactory>()?
                .CreateLogger("Axis.Kernel.FastEndpoints.AntiReplay");
            logger?.LogWarning("已启用防重放但未注册 ICacheService，已放行。");
            return;
        }

        var userId = http.User.FindFirst(ClaimConst.AccountId)?.Value;
        if (string.IsNullOrWhiteSpace(userId))
            userId = "0";

        // 按用户隔离 Nonce，降低跨用户碰撞面；TTL 与时钟窗口对齐，过窗后自然可复用。
        var cacheKey = $"{CacheCatalog.RequestNonce.KeyPrefix}{userId}:{nonce}";
        // 原子占坑：仅首次写入成功；并发重复 nonce 直接冲突。
        var acquired = await cache.TrySetAsync(cacheKey, 1, TimeSpan.FromSeconds(skewSeconds), ct);
        if (!acquired)
        {
            throw new BusinessException(
                ResultCodes.Conflict,
                "检测到重复请求，请勿重放",
                new { nonce });
        }
    }

    private static string? ExtractBearerToken(HttpContext http)
    {
        var header = http.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(header)) return null;

        const string prefix = "Bearer ";
        if (header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return header[prefix.Length..].Trim();

        return header.Trim();
    }
}
