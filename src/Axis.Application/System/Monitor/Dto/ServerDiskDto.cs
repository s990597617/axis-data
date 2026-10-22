namespace Axis.Application.System.Monitor.Dto;

/// <summary>
/// 磁盘分区信息。
/// </summary>
public sealed class ServerDiskDto
{
    /// <summary>
    /// 盘符或挂载名。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 卷标；可为空。
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// 文件系统类型。
    /// </summary>
    public string DriveFormat { get; set; } = null!;

    /// <summary>
    /// 总容量字节。
    /// </summary>
    public long TotalBytes { get; set; }

    /// <summary>
    /// 已用字节。
    /// </summary>
    public long UsedBytes { get; set; }

    /// <summary>
    /// 可用字节。
    /// </summary>
    public long FreeBytes { get; set; }

    /// <summary>
    /// 使用率（0-100）。
    /// </summary>
    public double Usage { get; set; }
}
