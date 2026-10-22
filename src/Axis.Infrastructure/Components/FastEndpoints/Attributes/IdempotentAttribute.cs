namespace Axis.Infrastructure.Components.FastEndpoints.Attributes;

/// <summary>
/// 声明一个 FastEndpoints 端点需要做请求幂等控制。
/// <para>
/// 客户端需在请求头（默认 <c>Idempotency-Key</c>）中附带一个业务维度唯一的 Key；
/// <see cref="Processors.IdempotencyPreProcessor"/> 会以
/// <c>"{endpointType}:{key}"</c> 为缓存键，在 <see cref="TtlSeconds"/> 秒内拒绝同 Key 的重复请求。
/// </para>
/// <para>
/// 典型场景：
/// </para>
/// <list type="bullet">
///   <item><description>下单 / 支付 / 退款等一次性操作，防止网络重试造成重复副作用。</description></item>
///   <item><description>外部 Webhook 回调，对方可能因超时重推相同事件。</description></item>
/// </list>
/// <example>
/// <code>
/// [Idempotent]                                 // 默认 120s 窗口，读取 Idempotency-Key
/// [Idempotent(TtlSeconds = 60)]                // 自定义 TTL
/// [Idempotent(HeaderName = "X-Request-Id")]    // 自定义请求头名
/// [Idempotent(Required = false)]               // Key 缺失时放行
/// </code>
/// </example>
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class IdempotentAttribute : Attribute
{
    /// <summary>
    /// 幂等窗口时长（秒）。默认 120 秒，覆盖绝大多数重试场景。
    /// <para>
    /// 值越大越防重，但会占用更多缓存；建议按业务重试策略的最大间隔设置。
    /// </para>
    /// </summary>
    public int TtlSeconds { get; init; } = 120;

    /// <summary>
    /// 用于读取幂等 Key 的请求头名，默认 <c>Idempotency-Key</c>（RFC 草案命名）。
    /// </summary>
    public string HeaderName { get; init; } = "Idempotency-Key";

    /// <summary>
    /// 是否强制要求客户端提供幂等 Key。
    /// <para>
    /// 默认 <c>true</c>：缺失时以 <see cref="Host.Responses.ResultCodes.BadRequest"/> 拒绝；
    /// 设为 <c>false</c> 时将静默放行（适合渐进式灰度：先上线再强制客户端传 Key）。
    /// </para>
    /// </summary>
    public bool Required { get; init; } = true;
}
