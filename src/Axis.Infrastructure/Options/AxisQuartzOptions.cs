namespace Axis.Infrastructure.Options;

/// <summary>
/// Quartz 调度选项。
/// </summary>
public sealed class AxisQuartzOptions
{
    /// <summary>
    /// 配置节名称。
    /// </summary>
    public const string SectionName = "Quartz";

    /// <summary>
    /// 是否启用调度器。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 调度器实例名。
    /// </summary>
    public string InstanceName { get; set; } = "AxisScheduler";

    /// <summary>
    /// 是否使用持久化 JobStore（需预先建好 QRTZ_ 表）；默认内存，配置以 sys_job 为准并在启动时同步。
    /// </summary>
    public bool UsePersistentStore { get; set; }

    /// <summary>
    /// 持久化表前缀。
    /// </summary>
    public string TablePrefix { get; set; } = "QRTZ_";

    /// <summary>
    /// 可选：覆盖数据库连接串（Npgsql 格式）。为空则尝试从 DbConnection 主库转换。
    /// </summary>
    public string ConnectionString { get; set; }
}
