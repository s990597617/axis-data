namespace Axis.Infrastructure.Components.Database.Internal;

/// <summary>
/// SqlSugar 使用常量
/// </summary>
public class SqlSugarConst
{
    /// <summary>
    /// 默认数据库标识
    /// </summary>
    public const string MainConfigId = "master";

    /// <summary>
    /// 日志库标识（需在 DbConnection.ConnectionConfigs 中配置同名 ConfigId）
    /// </summary>
    public const string LogConfigId = "log";

    /// <summary>
    /// 系统默认用户名
    /// </summary>
    public const string SuperAdminName = "AxisAdmin";
}
