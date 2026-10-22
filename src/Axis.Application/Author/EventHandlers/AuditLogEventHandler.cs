using Axis.Domain.System;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Components.EventBus;
using Axis.Infrastructure.Components.EventBus.Events;
using Microsoft.Extensions.Logging;
using Yitter.IdGenerator;

namespace Axis.Application.Author.EventHandlers;

/// <summary>
/// 审计日志持久化处理器。
/// <para>
/// 订阅 <see cref="AuditLogEvent"/>，根据日志类型分别写入对应的数据库表：
/// <list type="bullet">
///   <item><see cref="AuditLogType.Operate"/> → sys_log_op</item>
///   <item><see cref="AuditLogType.Visit"/> → sys_log_vis</item>
///   <item><see cref="AuditLogType.Error"/> → sys_log_error</item>
/// </list>
/// 异步写入，不阻塞请求响应。
/// </para>
/// </summary>
public class AuditLogEventHandler : AxisEventHandler<AuditLogEvent>
{
    private readonly IRepository<SysLogOp> _opRepo;
    private readonly IRepository<SysLogVis> _visRepo;
    private readonly IRepository<SysLogError> _errorRepo;
    private readonly ILogger<AuditLogEventHandler> _logger;

    public AuditLogEventHandler(
        IRepository<SysLogOp> opRepo,
        IRepository<SysLogVis> visRepo,
        IRepository<SysLogError> errorRepo,
        ILogger<AuditLogEventHandler> logger)
    {
        _opRepo = opRepo;
        _visRepo = visRepo;
        _errorRepo = errorRepo;
        _logger = logger;
    }

    protected override async Task OnHandleAsync(AuditLogEvent @event, CancellationToken ct)
    {
        switch (@event.LogType)
        {
            case AuditLogType.Operate:
                await PersistOperateLogAsync(@event, ct);
                break;
            case AuditLogType.Visit:
                await PersistVisitLogAsync(@event, ct);
                break;
            case AuditLogType.Error:
                await PersistErrorLogAsync(@event, ct);
                break;
        }
    }

    private async Task PersistOperateLogAsync(AuditLogEvent e, CancellationToken ct)
    {
        var entity = new SysLogOp
        {
            Id = YitIdHelper.NextId(),
            UserId = e.UserId,
            Account = e.Account,
            DisplayTitle = e.DisplayTitle,
            FullName = e.FullName,
            ActionName = e.ActionName,
            HttpMethod = e.HttpMethod,
            RequestUrl = e.RequestUrl,
            RequestParam = e.RequestParam,
            RemoteIp = e.RemoteIp,
            UserAgent = e.UserAgent,
            Elapsed = e.Elapsed,
            State = e.State,
            TraceId = e.TraceId,
            CreatedTime = DateTime.Now
        };

        await _opRepo.InsertAsync(entity, ct);
        _logger.LogDebug("操作日志已入库：{Url} by {Account}", e.RequestUrl, e.Account);
    }

    private async Task PersistVisitLogAsync(AuditLogEvent e, CancellationToken ct)
    {
        var entity = new SysLogVis
        {
            Id = YitIdHelper.NextId(),
            UserId = e.UserId,
            Account = e.Account,
            DisplayTitle = e.DisplayTitle,
            FullName = e.FullName,
            ActionName = e.ActionName,
            HttpMethod = e.HttpMethod,
            RequestUrl = e.RequestUrl,
            RequestParam = e.RequestParam,
            RemoteIp = e.RemoteIp,
            UserAgent = e.UserAgent,
            Elapsed = e.Elapsed,
            State = e.State,
            TraceId = e.TraceId,
            CreatedTime = DateTime.Now
        };

        await _visRepo.InsertAsync(entity, ct);
        _logger.LogDebug("访问日志已入库：{Url} by {Account}", e.RequestUrl, e.Account);
    }

    private async Task PersistErrorLogAsync(AuditLogEvent e, CancellationToken ct)
    {
        var entity = new SysLogError
        {
            Id = YitIdHelper.NextId(),
            DisplayTitle = e.DisplayTitle,
            FullName = e.FullName,
            ActionName = e.ActionName,
            HttpMethod = e.HttpMethod,
            RequestUrl = e.RequestUrl,
            RequestParam = e.RequestParam,
            RemoteIp = e.RemoteIp,
            UserAgent = e.UserAgent,
            Elapsed = e.Elapsed,
            Exception = e.Exception,
            TraceId = e.TraceId,
            ThreadId = e.ThreadId,
            CreatedTime = DateTime.Now
        };

        await _errorRepo.InsertAsync(entity, ct);
        _logger.LogDebug("异常日志已入库：{Url}", e.RequestUrl);
    }
}
