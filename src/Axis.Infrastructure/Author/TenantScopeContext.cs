namespace Axis.Infrastructure.Author;

/// <summary>
/// 租户作用域的异步局部存储。
/// </summary>
/// <remarks>
/// Job 与 Outbox 投递没有 HTTP 用户，用本上下文驱动 <c>ICurrentAccount.TenantId</c>
/// 与 SqlSugar 租户过滤器。
/// </remarks>
public static class TenantScopeContext
{
    private static readonly AsyncLocal<TenantScopeState?> CurrentState = new();

    /// <summary>
    /// 当前租户覆盖；未进入 <see cref="ITenantScope"/> 时为 <c>null</c>。
    /// </summary>
    public static TenantScopeState? Current => CurrentState.Value;

    /// <summary>
    /// 压入作用域；释放时恢复上一层。
    /// </summary>
    /// <param name="state">本层租户状态。</param>
    /// <returns>弹出上一层状态的释放器。</returns>
    public static IDisposable Push(TenantScopeState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var previous = CurrentState.Value;
        CurrentState.Value = state;
        return new Pop(previous);
    }

    private sealed class Pop(TenantScopeState? previous) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed) return;
            CurrentState.Value = previous;
            _disposed = true;
        }
    }
}
