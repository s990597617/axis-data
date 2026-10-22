namespace Axis.Infrastructure.Options;

/// <summary>
/// SignalR 通知推送配置（Config/12-SignalR.json）。
/// </summary>
public sealed class AxisSignalROptions
{
    public const string SectionName = "Axis:SignalR";

    /// <summary>
    /// 是否启用 SignalR；关闭时使用 NullUserNotifyPusher。
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Hub 路径，默认 /hubs/notify。
    /// </summary>
    public string HubPath { get; set; } = "/hubs/notify";
}
