using SqlSugar;

namespace Axis.Infrastructure.Options;

/// <summary>
/// 数据库连接配置根节点。
/// </summary>
public class AxisDbConnectionOptions
{
    /// <summary>
    /// 数据库连接集合。
    /// </summary>
    public List<AxisDbConnectionConfig> ConnectionConfigs { get; set; }
}

/// <summary>
/// 单库连接配置（扩展 SqlSugar <see cref="ConnectionConfig"/>）。
/// </summary>
public class AxisDbConnectionConfig : ConnectionConfig
{
    /// <summary>
    /// Axis 库表初始化与命名等扩展设置。
    /// </summary>
    public AxisDbSettings DbSettings { get; set; }
}

/// <summary>
/// 数据库初始化与命名约定配置。
/// </summary>
public class AxisDbSettings
{
    /// <summary>
    /// 启用库初始化（建库）。
    /// </summary>
    public bool EnableInitDb { get; set; }

    /// <summary>
    /// 启用库表差异日志。
    /// </summary>
    public bool EnableDiffLog { get; set; }

    /// <summary>
    /// 启用驼峰转下划线。
    /// </summary>
    public bool EnableUnderLine { get; set; }

    /// <summary>
    /// 启用表结构初始化。
    /// </summary>
    public bool EnableInitTable { get; set; }

    /// <summary>
    /// 启用种子数据初始化。
    /// </summary>
    public bool EnableInitSeed { get; set; }
}
