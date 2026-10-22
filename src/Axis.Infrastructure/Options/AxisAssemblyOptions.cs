namespace Axis.Infrastructure.Options;

/// <summary>
/// 启动扫描用的程序集配置。
/// </summary>
public class AxisAssemblyOptions
{
    /// <summary>
    /// SqlSugar 种子 / 实体扫描程序集（| 分隔）。
    /// </summary>
    public string SqlSugar { get; set; }

    /// <summary>
    /// FastEndpoints 扫描程序集（| 分隔）。
    /// </summary>
    public string FastEndPoints { get; set; }

    /// <summary>
    /// Mapster 映射注册扫描程序集（| 分隔）。
    /// </summary>
    public string Mapster { get; set; }
}
