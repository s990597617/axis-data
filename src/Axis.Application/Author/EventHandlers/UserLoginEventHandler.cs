using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Components.EventBus;
using Axis.Infrastructure.Components.EventBus.Events;
using Microsoft.Extensions.Logging;
using Yitter.IdGenerator;

namespace Axis.Application.Author.EventHandlers;

/// <summary>
/// 用户登录事件处理器：持久化登录日志到 sys_log_login 表。
/// <para>
/// 当 <see cref="UserLoginEvent"/> 被发布时，本 Handler 自动被调用。
/// 无需任何注册代码，FastEndpoints 自动扫描发现。
/// </para>
/// </summary>
public class UserLoginEventHandler : AxisEventHandler<UserLoginEvent>
{
    private readonly IRepository<SysLogLogin> _loginLogRepo;
    private readonly ILogger<UserLoginEventHandler> _logger;

    public UserLoginEventHandler(
        IRepository<SysLogLogin> loginLogRepo,
        ILogger<UserLoginEventHandler> logger)
    {
        _loginLogRepo = loginLogRepo;
        _logger = logger;
    }

    protected override async Task OnHandleAsync(UserLoginEvent @event, CancellationToken ct)
    {
        var entity = new SysLogLogin
        {
            Id = YitIdHelper.NextId(),
            UserId = @event.UserId,
            Account = @event.Account,
            RemoteIp = @event.RemoteIp,
            UserAgent = @event.UserAgent,
            Success = true,
            CreatedTime = DateTime.Now
        };

        await _loginLogRepo.InsertAsync(entity, ct);

        _logger.LogInformation(
            "登录日志已入库：UserId={UserId}, Account={Account}, IP={IP}",
            @event.UserId, @event.Account, @event.RemoteIp);
    }
}
