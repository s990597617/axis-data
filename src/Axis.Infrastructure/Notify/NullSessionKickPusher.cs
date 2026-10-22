using Axis.Infrastructure.Dependency.Annotations;

namespace Axis.Infrastructure.Notify;

/// <summary>
/// 未启用 SignalR 时的空踢人实现。
/// </summary>
public sealed class NullSessionKickPusher : ISessionKickPusher, IAutoInject
{
    /// <inheritdoc />
    public Task PushForceLogoutAsync(long userId, ForceLogoutPayload payload, CancellationToken ct = default)
        => Task.CompletedTask;
}
