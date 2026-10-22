using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using IoFile = global::System.IO.File;

namespace Axis.Application.System.Monitor.Internal;

/// <summary>
/// 跨平台系统指标读取（CPU / 物理内存）。
/// </summary>
internal static class SystemMetricsReader
{
    private static readonly object Sync = new();
    private static DateTime _lastProcessSampleAt;
    private static TimeSpan _lastProcessCpu;
    private static DateTime _lastSystemSampleAt;
    private static long _lastSystemIdle;
    private static long _lastSystemTotal;

    /// <summary>
    /// 计算当前进程 CPU 使用率（基于两次采样差值）。
    /// </summary>
    public static double GetProcessCpuUsage()
    {
        using var process = Process.GetCurrentProcess();
        var cpu = process.TotalProcessorTime;
        var now = DateTime.UtcNow;

        lock (Sync)
        {
            if (_lastProcessSampleAt == default)
            {
                _lastProcessSampleAt = now;
                _lastProcessCpu = cpu;
                return 0;
            }

            var cpuDeltaMs = (cpu - _lastProcessCpu).TotalMilliseconds;
            var wallDeltaMs = (now - _lastProcessSampleAt).TotalMilliseconds * Environment.ProcessorCount;
            _lastProcessSampleAt = now;
            _lastProcessCpu = cpu;
            if (wallDeltaMs <= 0) return 0;
            return Math.Clamp(Math.Round(cpuDeltaMs / wallDeltaMs * 100, 2), 0, 100);
        }
    }

    /// <summary>
    /// 尝试读取系统 CPU 使用率；不支持时返回 null。
    /// </summary>
    public static double? TryGetSystemCpuUsage()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return TryGetWindowsSystemCpuUsage();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return TryGetLinuxSystemCpuUsage();
        return null;
    }

    /// <summary>
    /// 尝试读取系统物理内存（总量、可用量）；不支持时返回 null。
    /// </summary>
    public static (long Total, long Available)? TryGetSystemMemory()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return TryGetWindowsMemory();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return TryGetLinuxMemory();
        return null;
    }

    [SupportedOSPlatform("windows")]
    private static double? TryGetWindowsSystemCpuUsage()
    {
        if (!GetSystemTimes(out var idle, out var kernel, out var user))
            return null;

        var idleTime = FileTimeToLong(idle);
        var totalTime = FileTimeToLong(kernel) + FileTimeToLong(user);
        var now = DateTime.UtcNow;

        lock (Sync)
        {
            if (_lastSystemSampleAt == default)
            {
                _lastSystemSampleAt = now;
                _lastSystemIdle = idleTime;
                _lastSystemTotal = totalTime;
                return null;
            }

            var idleDelta = idleTime - _lastSystemIdle;
            var totalDelta = totalTime - _lastSystemTotal;
            _lastSystemSampleAt = now;
            _lastSystemIdle = idleTime;
            _lastSystemTotal = totalTime;
            if (totalDelta <= 0) return 0;
            var usage = (1.0 - idleDelta / (double)totalDelta) * 100;
            return Math.Clamp(Math.Round(usage, 2), 0, 100);
        }
    }

    private static double? TryGetLinuxSystemCpuUsage()
    {
        if (!TryReadLinuxCpu(out var idle, out var total))
            return null;

        var now = DateTime.UtcNow;
        lock (Sync)
        {
            if (_lastSystemSampleAt == default)
            {
                _lastSystemSampleAt = now;
                _lastSystemIdle = idle;
                _lastSystemTotal = total;
                return null;
            }

            var idleDelta = idle - _lastSystemIdle;
            var totalDelta = total - _lastSystemTotal;
            _lastSystemSampleAt = now;
            _lastSystemIdle = idle;
            _lastSystemTotal = total;
            if (totalDelta <= 0) return 0;
            var usage = (1.0 - idleDelta / (double)totalDelta) * 100;
            return Math.Clamp(Math.Round(usage, 2), 0, 100);
        }
    }

    private static bool TryReadLinuxCpu(out long idle, out long total)
    {
        idle = 0;
        total = 0;
        try
        {
            var line = IoFile.ReadLines("/proc/stat").FirstOrDefault();
            if (line is null || !line.StartsWith("cpu ", StringComparison.Ordinal))
                return false;

            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 5) return false;

            long sum = 0;
            for (var i = 1; i < parts.Length; i++)
            {
                if (!long.TryParse(parts[i], out var value)) return false;
                sum += value;
                if (i == 4) idle = value;
            }

            total = sum;
            return true;
        }
        catch
        {
            return false;
        }
    }

    [SupportedOSPlatform("windows")]
    private static (long Total, long Available)? TryGetWindowsMemory()
    {
        var status = new MemoryStatusEx { Length = (uint)Marshal.SizeOf<MemoryStatusEx>() };
        if (!GlobalMemoryStatusEx(ref status)) return null;
        return ((long)status.TotalPhys, (long)status.AvailPhys);
    }

    private static (long Total, long Available)? TryGetLinuxMemory()
    {
        try
        {
            long? total = null;
            long? available = null;
            foreach (var line in global::System.IO.File.ReadLines("/proc/meminfo"))
            {
                if (line.StartsWith("MemTotal:", StringComparison.Ordinal) && TryParseMemKb(line, out var t))
                    total = t;
                else if (line.StartsWith("MemAvailable:", StringComparison.Ordinal) && TryParseMemKb(line, out var a))
                    available = a;

                if (total.HasValue && available.HasValue)
                    return (total.Value, available.Value);
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    private static bool TryParseMemKb(string line, out long bytes)
    {
        bytes = 0;
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2 || !long.TryParse(parts[1], out var kb)) return false;
        bytes = kb * 1024;
        return true;
    }

    private static long FileTimeToLong(FileTime ft)
        => ((long)ft.DwHighDateTime << 32) | ft.DwLowDateTime;

    [StructLayout(LayoutKind.Sequential)]
    private struct FileTime
    {
        public uint DwLowDateTime;
        public uint DwHighDateTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MemoryStatusEx
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhys;
        public ulong AvailPhys;
        public ulong TotalPageFile;
        public ulong AvailPageFile;
        public ulong TotalVirtual;
        public ulong AvailVirtual;
        public ulong AvailExtendedVirtual;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [SupportedOSPlatform("windows")]
    private static extern bool GetSystemTimes(out FileTime idleTime, out FileTime kernelTime, out FileTime userTime);

    [DllImport("kernel32.dll", SetLastError = true)]
    [SupportedOSPlatform("windows")]
    private static extern bool GlobalMemoryStatusEx(ref MemoryStatusEx buffer);
}
