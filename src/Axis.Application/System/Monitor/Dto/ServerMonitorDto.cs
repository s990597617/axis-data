namespace Axis.Application.System.Monitor.Dto;

/// <summary>
/// 服务监控聚合数据：CPU / 内存 / 磁盘 / 运行时与系统信息。
/// </summary>
public sealed class ServerMonitorDto
{
    /// <summary>
    /// 采集时间。
    /// </summary>
    public DateTime CollectedAt { get; set; }

    /// <summary>
    /// CPU 信息。
    /// </summary>
    public ServerCpuDto Cpu { get; set; } = new();

    /// <summary>
    /// 内存信息。
    /// </summary>
    public ServerMemoryDto Memory { get; set; } = new();

    /// <summary>
    /// 磁盘列表。
    /// </summary>
    public List<ServerDiskDto> Disks { get; set; } = [];

    /// <summary>
    /// 运行时与进程信息。
    /// </summary>
    public ServerRuntimeDto Runtime { get; set; } = new();

    /// <summary>
    /// 操作系统与主机信息。
    /// </summary>
    public ServerOsDto Os { get; set; } = new();
}
