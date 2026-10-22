namespace Axis.Infrastructure.Options;

/// <summary>
/// 短信发送配置。
/// </summary>
public sealed class AxisSmsOptions
{
    /// <summary>
    /// 配置节名称。
    /// </summary>
    public const string SectionName = "Sms";

    /// <summary>
    /// 是否启用短信通道；关闭时注册空实现。
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 提供者名称：<c>None</c> / <c>Logging</c>；后续可扩展 <c>Aliyun</c> / <c>Tencent</c> 等。
    /// </summary>
    public string Provider { get; set; } = "None";

    /// <summary>
    /// 默认短信签名（部分云厂商必填）。
    /// </summary>
    public string DefaultSignName { get; set; } = string.Empty;

    /// <summary>
    /// 厂商扩展参数（AccessKey、Region、Endpoint 等），由具体 Provider 解读。
    /// </summary>
    public Dictionary<string, string> Settings { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
