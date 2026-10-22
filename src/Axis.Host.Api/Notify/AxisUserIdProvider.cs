using Axis.Infrastructure.Author.Internal;
using Microsoft.AspNetCore.SignalR;

namespace Axis.Host.Api.Notify;

/// <summary>
/// 将 SignalR 连接映射到 JWT Claim <c>id</c>（与 CurrentAccount.Id 对齐）。
/// </summary>
public sealed class AxisUserIdProvider : IUserIdProvider
{
    /// <inheritdoc />
    public string GetUserId(HubConnectionContext connection)
        => connection.User?.FindFirst(ClaimConst.AccountId)?.Value;
}
