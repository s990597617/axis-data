using Axis.Application.System.Monitor.Dto;

namespace Axis.Application.System.Monitor;

/// <summary>
/// 服务监控应用服务契约。
/// </summary>
public interface IServerMonitorAppService
{
    /// <summary>
    /// 采集当前主机与进程的监控快照。
    /// </summary>
    Task<ServerMonitorDto> GetAsync(CancellationToken ct = default);
}
