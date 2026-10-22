namespace Axis.Infrastructure.Author;

/// <summary>
/// 后台任务 / Outbox 投递使用的租户覆盖状态。
/// </summary>
/// <remarks>
/// 由 <see cref="ITenantScope"/> 写入 <see cref="TenantScopeContext"/>；
/// HTTP 请求无覆盖时仍以 JWT 为准。
/// </remarks>
public sealed class TenantScopeState
{
    /// <summary>
    /// 当前作用域租户 Id；<see cref="Unrestricted"/> 为真时忽略。
    /// </summary>
    public long? TenantId { get; init; }

    /// <summary>
    /// 不挂租户过滤器，等价于跨租户（平台任务）。
    /// </summary>
    public bool Unrestricted { get; init; }
}
