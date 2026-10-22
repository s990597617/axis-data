namespace Axis.Application.System.Monitor.Dto;

/// <summary>
/// 操作系统与主机信息。
/// </summary>
public sealed class ServerOsDto
{
    /// <summary>
    /// 主机名。
    /// </summary>
    public string MachineName { get; set; } = null!;

    /// <summary>
    /// 操作系统描述。
    /// </summary>
    public string OsDescription { get; set; } = null!;

    /// <summary>
    /// OS 架构。
    /// </summary>
    public string OsArchitecture { get; set; } = null!;

    /// <summary>
    /// 本机 IPv4 列表（逗号分隔）。
    /// </summary>
    public string IpAddresses { get; set; } = null!;

    /// <summary>
    /// 应用内容根路径。
    /// </summary>
    public string ContentRoot { get; set; } = null!;
}
