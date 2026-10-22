namespace Axis.Infrastructure.Author;

/// <summary>
/// 为无 HTTP 上下文的代码临时设置当前租户。
/// </summary>
/// <remarks>
/// Quartz、Outbox Dispatcher 必须经本接口进入业务；禁止依赖空的
/// <see cref="ICurrentAccount"/> 隐式串租户。
/// </remarks>
public interface ITenantScope
{
    /// <summary>
    /// 在指定租户下执行；<paramref name="tenantId"/> 为空表示不挂租户过滤。
    /// </summary>
    /// <param name="tenantId">租户 Id；空则跨租户。</param>
    /// <param name="action">业务委托。</param>
    /// <param name="ct">取消令牌。</param>
    Task RunAsync(long? tenantId, Func<CancellationToken, Task> action, CancellationToken ct = default);

    /// <summary>
    /// 在指定租户下执行并返回结果。
    /// </summary>
    /// <typeparam name="T">返回类型。</typeparam>
    /// <param name="tenantId">租户 Id；空则跨租户。</param>
    /// <param name="action">业务委托。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>委托返回值。</returns>
    Task<T> RunAsync<T>(long? tenantId, Func<CancellationToken, Task<T>> action, CancellationToken ct = default);
}
