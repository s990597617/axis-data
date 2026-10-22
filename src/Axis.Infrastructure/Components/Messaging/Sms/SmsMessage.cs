namespace Axis.Infrastructure.Components.Messaging.Sms;

/// <summary>
/// 待发送短信。
/// </summary>
public sealed class SmsMessage
{
    /// <summary>
    /// 手机号列表（国内号建议不带 +86，由 Provider 自行规范化）。
    /// </summary>
    public required IReadOnlyList<string> PhoneNumbers { get; init; }

    /// <summary>
    /// 纯文本内容；模板发送时可空。
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// 云厂商模板编码；模板发送时必填。
    /// </summary>
    public string? TemplateCode { get; init; }

    /// <summary>
    /// 模板变量；键值由各厂商约定。
    /// </summary>
    public IReadOnlyDictionary<string, string>? TemplateParams { get; init; }

    /// <summary>
    /// 短信签名；为空时使用配置默认签名。
    /// </summary>
    public string? SignName { get; init; }
}
