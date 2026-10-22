namespace Axis.Application.System.Monitor.Dto;

/// <summary>
/// 内存使用情况（字节）。
/// </summary>
public sealed class ServerMemoryDto
{
    /// <summary>
    /// 系统总物理内存；无法采集时为 null。
    /// </summary>
    public long? TotalBytes { get; set; }

    /// <summary>
    /// 系统已用物理内存；无法采集时为 null。
    /// </summary>
    public long? UsedBytes { get; set; }

    /// <summary>
    /// 系统可用物理内存；无法采集时为 null。
    /// </summary>
    public long? AvailableBytes { get; set; }

    /// <summary>
    /// 系统内存使用率（0-100）；无法采集时为 null。
    /// </summary>
    public double? Usage { get; set; }

    /// <summary>
    /// 当前进程工作集（Working Set）。
    /// </summary>
    public long ProcessWorkingSetBytes { get; set; }

    /// <summary>
    /// 当前进程私有内存。
    /// </summary>
    public long ProcessPrivateBytes { get; set; }

    /// <summary>
    /// CLR 托管堆已分配字节。
    /// </summary>
    public long ManagedHeapBytes { get; set; }
}
