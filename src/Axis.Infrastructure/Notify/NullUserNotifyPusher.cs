using Axis.Infrastructure.Dependency.Annotations;

namespace Axis.Infrastructure.Notify;

/// <summary>
/// 未注册 SignalR 时的空推送实现，保证业务用例可离线跑通。
/// </summary>
/// <remarks>
/// 由程序集扫描自动注册；Host 在 DI 扫描前通过 <c>TryAdd</c> 注册 SignalR 实现时可将其覆盖。
/// </remarks>
public sealed class NullUserNotifyPusher : IUserNotifyPusher, IAutoInject
{
    /// <inheritdoc />
    public Task PushUnreadAsync(long userId, UnreadNotifyPayload payload, CancellationToken ct = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public Task PushUnreadAsync(
        IEnumerable<long> userIds,
        Func<long, Task<UnreadNotifyPayload>> payloadFactory,
        CancellationToken ct = default)
        => Task.CompletedTask;
}
