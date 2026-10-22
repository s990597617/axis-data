using Axis.Infrastructure.Components.Caching.Services;
using Axis.Infrastructure.Components.FastEndpoints.Attributes;
using Axis.Infrastructure.Constants;
using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Host.Responses;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Axis.Infrastructure.Components.FastEndpoints.Processors;

/// <summary>
/// 幂等前置处理器。
/// <para>
/// 仅对标注了 <see cref="IdempotentAttribute"/> 的端点生效，
/// 通过 <see cref="ICacheService"/> 以 "请求头 Key + 端点类型" 作为缓存键实现"短时间内同 Key 只放行一次"。
/// </para>
/// <para>
/// 处理流程：
/// </para>
/// <list type="number">
///   <item><description>从请求头读取幂等 Key；缺失时按 <see cref="IdempotentAttribute.Required"/> 决定拒绝或放行。</description></item>
///   <item><description>使用 <c>TrySetAsync</c> 原子占坑；失败则以 <see cref="ResultCodes.Conflict"/> 拒绝（409）。</description></item>
/// </list>
/// <para>
/// 对强一致幂等（如支付）仍建议在业务层叠加数据库唯一索引作为兜底。
/// </para>
/// </summary>
public sealed class IdempotencyPreProcessor : IGlobalPreProcessor
{
    /// <summary>
    /// 处理器进入点：按特性配置校验请求头中的幂等 Key 并登记缓存占位。
    /// </summary>
    /// <param name="context">FastEndpoints 前置处理器上下文。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task PreProcessAsync(IPreProcessorContext context, CancellationToken ct)
    {
        var http = context.HttpContext;

        // 同时兼容类级 [Idempotent] 与 Options(x => x.WithMetadata(new IdempotentAttribute(...))) 两种写法
        var attr = EndpointMetadataResolver.Resolve<IdempotentAttribute>(http);
        if (attr is null) return;

        // 1) 读取 Key
        var key = http.Request.Headers[attr.HeaderName].ToString();
        if (string.IsNullOrWhiteSpace(key))
        {
            if (!attr.Required) return;
            throw new BusinessException(
                ResultCodes.BadRequest,
                $"缺少幂等请求头：{attr.HeaderName}");
        }

        // 2) 缓存原子占坑
        var cache = http.RequestServices.GetService<ICacheService>();
        var endpointType = EndpointMetadataResolver.ResolveEndpointType(http);
        if (cache is null)
        {
            var logger = http.RequestServices.GetService<ILoggerFactory>()?
                .CreateLogger("Axis.Kernel.FastEndpoints.Idempotency");
            logger?.LogWarning("端点 {Endpoint} 声明了 [Idempotent] 但未注册 ICacheService，已放行。",
                endpointType?.FullName);
            return;
        }

        var cacheKey = $"{CacheCatalog.Idempotency.KeyPrefix}:{endpointType?.FullName ?? "unknown"}:{key}";
        var acquired = await cache.TrySetAsync(cacheKey, "1", TimeSpan.FromSeconds(attr.TtlSeconds), ct);
        if (!acquired)
        {
            throw new BusinessException(
                ResultCodes.Conflict,
                "请求已处理，请勿重复提交",
                new { idempotencyKey = key });
        }
    }
}
