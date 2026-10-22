namespace Axis.Application.System.Monitor.Dto;

/// <summary>
/// 运行时与进程信息。
/// </summary>
public sealed class ServerRuntimeDto
{
    /// <summary>
    /// .NET 运行时描述。
    /// </summary>
    public string FrameworkDescription { get; set; } = null!;

    /// <summary>
    /// 进程架构（X64 / Arm64 等）。
    /// </summary>
    public string ProcessArchitecture { get; set; } = null!;

    /// <summary>
    /// 进程 Id。
    /// </summary>
    public int ProcessId { get; set; }

    /// <summary>
    /// 进程名。
    /// </summary>
    public string ProcessName { get; set; } = null!;

    /// <summary>
    /// 进程启动时间。
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 已运行时长（秒）。
    /// </summary>
    public long UptimeSeconds { get; set; }

    /// <summary>
    /// 线程数。
    /// </summary>
    public int ThreadCount { get; set; }

    /// <summary>
    /// 句柄数（部分平台可能为 0）。
    /// </summary>
    public int HandleCount { get; set; }

    /// <summary>
    /// 宿主环境名（Development / Production 等）。
    /// </summary>
    public string EnvironmentName { get; set; } = null!;

    /// <summary>
    /// 是否 Server GC。
    /// </summary>
    public bool IsServerGc { get; set; }

    /// <summary>
    /// GC Gen0 回收次数。
    /// </summary>
    public int GcGen0Collections { get; set; }

    /// <summary>
    /// GC Gen1 回收次数。
    /// </summary>
    public int GcGen1Collections { get; set; }

    /// <summary>
    /// GC Gen2 回收次数。
    /// </summary>
    public int GcGen2Collections { get; set; }
}
