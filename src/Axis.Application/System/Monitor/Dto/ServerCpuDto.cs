namespace Axis.Application.System.Monitor.Dto;

/// <summary>
/// CPU 使用情况。
/// </summary>
public sealed class ServerCpuDto
{
    /// <summary>
    /// 逻辑处理器数量。
    /// </summary>
    public int ProcessorCount { get; set; }

    /// <summary>
    /// 当前进程 CPU 使用率（0-100）；首次采样可能为 0。
    /// </summary>
    public double ProcessUsage { get; set; }

    /// <summary>
    /// 系统 CPU 使用率（0-100）；无法采集时为 null。
    /// </summary>
    public double? SystemUsage { get; set; }
}
