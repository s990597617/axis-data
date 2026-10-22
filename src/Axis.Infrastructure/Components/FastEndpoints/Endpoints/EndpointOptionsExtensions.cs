using System.Data;
using Axis.Infrastructure.Components.FastEndpoints.Attributes;
using Axis.Infrastructure.Security.OpenApi;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;

namespace Axis.Infrastructure.Components.FastEndpoints.Endpoints;

/// <summary>
/// Axis 约定的端点 <see cref="RouteHandlerBuilder"/> 流式扩展。
/// <para>
/// 与 FastEndpoints 官方推荐一致，所有业务横切特性统一通过
/// <c>Options(x =&gt; x.WithMetadata(...))</c> 注入到端点元数据集合，
/// 本扩展只是对 <c>WithMetadata(new XxxAttribute(...))</c> 的语义封装，
/// 让 <c>Configure()</c> 方法读起来更像自然语言，并避免每次手 new 特性实例。
/// </para>
/// <para>
/// 所有扩展都只依赖公共的 <see cref="RouteHandlerBuilder"/> 管道，
/// 不对 FastEndpoints 内部 API 产生耦合，方便随 FE 升级而平滑迭代。
/// </para>
/// </summary>
public static class EndpointOptionsExtensions
{
    /// <summary>
    /// 声明端点需要的业务权限码（同时具备所有码才放行）。
    /// <para>
    /// 等价写法：<c>x.WithMetadata(new PermissionAttribute(codes))</c>。
    /// </para>
    /// </summary>
    /// <param name="builder">FastEndpoints 的 <c>Options(x =&gt; ...)</c> 构建器。</param>
    /// <param name="codes">必须同时具备的权限码列表。</param>
    /// <returns>同一个构建器，便于继续链式调用。</returns>
    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder, params string[] codes)
        => builder.WithMetadata(new PermissionAttribute(codes));

    /// <summary>
    /// 声明端点需要的业务权限码，并指定判定策略（AllOf / AnyOf）。
    /// <para>
    /// 等价写法：<c>x.WithMetadata(new PermissionAttribute(policy, codes))</c>。
    /// </para>
    /// </summary>
    /// <param name="builder">FastEndpoints 的 <c>Options(x =&gt; ...)</c> 构建器。</param>
    /// <param name="policy">权限码组合策略。</param>
    /// <param name="codes">权限码列表。</param>
    /// <returns>同一个构建器，便于继续链式调用。</returns>
    public static RouteHandlerBuilder RequirePermission(this RouteHandlerBuilder builder, PermissionPolicy policy, params string[] codes)
        => builder.WithMetadata(new PermissionAttribute(policy, codes));

    /// <summary>
    /// 声明端点需要在统一数据库事务中执行，由 Axis 事务前后置处理器自动驱动。
    /// <para>
    /// 等价写法：<c>x.WithMetadata(new TransactionalAttribute { ... })</c>。
    /// </para>
    /// </summary>
    /// <param name="builder">FastEndpoints 的 <c>Options(x =&gt; ...)</c> 构建器。</param>
    /// <param name="isolationLevel">事务隔离级别，默认 <see cref="IsolationLevel.ReadCommitted"/>。</param>
    /// <param name="connectionId">参与事务的连接 ConfigId，空表示使用默认连接。</param>
    /// <returns>同一个构建器，便于继续链式调用。</returns>
    public static RouteHandlerBuilder Transactional(
        this RouteHandlerBuilder builder,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        string connectionId = null)
        => builder.WithMetadata(new TransactionalAttribute
        {
            IsolationLevel = isolationLevel,
            ConnectionId = connectionId
        });

    /// <summary>
    /// 声明端点需要请求幂等控制（按请求头中的幂等 Key 去重）。
    /// <para>
    /// 等价写法：<c>x.WithMetadata(new IdempotentAttribute { ... })</c>。
    /// </para>
    /// </summary>
    /// <param name="builder">FastEndpoints 的 <c>Options(x =&gt; ...)</c> 构建器。</param>
    /// <param name="ttlSeconds">幂等窗口时长（秒），默认 120。</param>
    /// <param name="headerName">承载幂等 Key 的请求头名，默认 <c>Idempotency-Key</c>。</param>
    /// <param name="required">缺失 Key 时是否拒绝，默认 <c>true</c>。</param>
    /// <returns>同一个构建器，便于继续链式调用。</returns>
    public static RouteHandlerBuilder Idempotent(
        this RouteHandlerBuilder builder,
        int ttlSeconds = 120,
        string headerName = "Idempotency-Key",
        bool required = true)
        => builder.WithMetadata(new IdempotentAttribute
        {
            TtlSeconds = ttlSeconds,
            HeaderName = headerName,
            Required = required
        });

    /// <summary>
    /// 声明端点跳过请求级时间戳 + Nonce 防重放（对外 OpenAPI / Webhook 等）。
    /// <para>
    /// 等价写法：<c>x.WithMetadata(new SkipAntiReplayAttribute(reason))</c>。
    /// </para>
    /// </summary>
    /// <param name="builder">FastEndpoints 的 <c>Options(x =&gt; ...)</c> 构建器。</param>
    /// <param name="reason">跳过原因（仅说明用途，可选）。</param>
    /// <returns>同一个构建器，便于继续链式调用。</returns>
    public static RouteHandlerBuilder SkipAntiReplay(
        this RouteHandlerBuilder builder,
        string? reason = null)
        => builder.WithMetadata(
            string.IsNullOrWhiteSpace(reason)
                ? new SkipAntiReplayAttribute()
                : new SkipAntiReplayAttribute(reason));
}

/// <summary>
/// FastEndpoints 端点认证方案扩展（OpenAPI Signature / JWT）。
/// </summary>
public static class EndpointAuthExtensions
{
    /// <summary>
    /// 仅接受 OpenAPI HMAC 签名认证。
    /// </summary>
    public static void RequireSignatureAuth(this EndpointDefinition definition)
        => definition.AuthSchemes(SignatureAuthenticationDefaults.AuthenticationScheme);

    /// <summary>
    /// 同时接受 JWT Bearer 与 OpenAPI Signature（任一通过即可）。
    /// </summary>
    public static void AllowJwtOrSignature(this EndpointDefinition definition)
        => definition.AuthSchemes(
            "Bearer",
            SignatureAuthenticationDefaults.AuthenticationScheme);
}
