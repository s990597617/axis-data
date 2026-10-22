using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime;
using System.Runtime.InteropServices;
using Axis.Application.System.Monitor.Dto;
using Axis.Application.System.Monitor.Internal;
using Axis.Infrastructure.Dependency.Annotations;
using Microsoft.AspNetCore.Hosting;

namespace Axis.Application.System.Monitor;

/// <summary>
/// 服务监控应用服务：采集本机 CPU / 内存 / 磁盘 / 运行时信息。
/// </summary>
public sealed class ServerMonitorAppService(IWebHostEnvironment environment)
    : IServerMonitorAppService, IAutoInject
{
    /// <inheritdoc />
    public Task<ServerMonitorDto> GetAsync(CancellationToken ct = default)
    {
        using var process = Process.GetCurrentProcess();
        process.Refresh();

        var memory = BuildMemory(process);
        var dto = new ServerMonitorDto
        {
            CollectedAt = DateTime.Now,
            Cpu = new ServerCpuDto
            {
                ProcessorCount = Environment.ProcessorCount,
                ProcessUsage = SystemMetricsReader.GetProcessCpuUsage(),
                SystemUsage = SystemMetricsReader.TryGetSystemCpuUsage()
            },
            Memory = memory,
            Disks = BuildDisks(),
            Runtime = BuildRuntime(process),
            Os = BuildOs()
        };

        return Task.FromResult(dto);
    }

    private static ServerMemoryDto BuildMemory(Process process)
    {
        var dto = new ServerMemoryDto
        {
            ProcessWorkingSetBytes = process.WorkingSet64,
            ProcessPrivateBytes = process.PrivateMemorySize64,
            ManagedHeapBytes = GC.GetTotalMemory(forceFullCollection: false)
        };

        var system = SystemMetricsReader.TryGetSystemMemory();
        if (system is null) return dto;

        var (total, available) = system.Value;
        var used = Math.Max(0, total - available);
        dto.TotalBytes = total;
        dto.AvailableBytes = available;
        dto.UsedBytes = used;
        dto.Usage = total <= 0 ? 0 : Math.Round(used * 100.0 / total, 2);
        return dto;
    }

    private static List<ServerDiskDto> BuildDisks()
    {
        var disks = new List<ServerDiskDto>();
        foreach (var drive in DriveInfo.GetDrives())
        {
            try
            {
                if (!drive.IsReady) continue;
                // 跳过光驱/网络盘等无意义或易超时的卷
                if (drive.DriveType is DriveType.CDRom or DriveType.Network or DriveType.NoRootDirectory)
                    continue;

                var total = drive.TotalSize;
                var free = drive.TotalFreeSpace;
                var used = Math.Max(0, total - free);
                disks.Add(new ServerDiskDto
                {
                    Name = drive.Name.TrimEnd('\\', '/'),
                    Label = string.IsNullOrWhiteSpace(drive.VolumeLabel) ? null : drive.VolumeLabel,
                    DriveFormat = drive.DriveFormat,
                    TotalBytes = total,
                    FreeBytes = free,
                    UsedBytes = used,
                    Usage = total <= 0 ? 0 : Math.Round(used * 100.0 / total, 2)
                });
            }
            catch
            {
                // 个别盘不可读时跳过
            }
        }

        return disks;
    }

    private ServerRuntimeDto BuildRuntime(Process process)
    {
        var start = process.StartTime.ToLocalTime();
        var uptime = DateTime.Now - start;
        return new ServerRuntimeDto
        {
            FrameworkDescription = RuntimeInformation.FrameworkDescription,
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            ProcessId = process.Id,
            ProcessName = process.ProcessName,
            StartTime = start,
            UptimeSeconds = (long)Math.Max(0, uptime.TotalSeconds),
            ThreadCount = process.Threads.Count,
            HandleCount = SafeHandleCount(process),
            EnvironmentName = environment.EnvironmentName,
            IsServerGc = GCSettings.IsServerGC,
            GcGen0Collections = GC.CollectionCount(0),
            GcGen1Collections = GC.CollectionCount(1),
            GcGen2Collections = GC.CollectionCount(2)
        };
    }

    private ServerOsDto BuildOs()
        => new()
        {
            MachineName = Environment.MachineName,
            OsDescription = RuntimeInformation.OSDescription,
            OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
            IpAddresses = string.Join(", ", GetLocalIPv4Addresses()),
            ContentRoot = environment.ContentRootPath
        };

    private static int SafeHandleCount(Process process)
    {
        try
        {
            return process.HandleCount;
        }
        catch
        {
            return 0;
        }
    }

    private static IEnumerable<string> GetLocalIPv4Addresses()
    {
        try
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(nic =>
                    nic.OperationalStatus == OperationalStatus.Up
                    && nic.NetworkInterfaceType is not NetworkInterfaceType.Loopback)
                .SelectMany(nic => nic.GetIPProperties().UnicastAddresses)
                .Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(addr => addr.Address.ToString())
                .Distinct()
                .OrderBy(ip => ip);
        }
        catch
        {
            try
            {
                return Dns.GetHostAddresses(Dns.GetHostName())
                    .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                    .Select(ip => ip.ToString());
            }
            catch
            {
                return [];
            }
        }
    }
}
