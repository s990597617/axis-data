using Microsoft.Extensions.Logging;

namespace Axis.Infrastructure.Components.Messaging.Sms;

/// <summary>
/// 未启用短信时的空实现：记录调试日志并返回成功。
/// </summary>
public sealed class NullSmsSender(ILogger<NullSmsSender> logger) : ISmsSender
{
    /// <inheritdoc />
    public Task<SmsSendResult> SendAsync(SmsMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        logger.LogDebug(
            "NullSmsSender 跳过发送：Phones={Phones} Template={Template} ContentLength={Length}",
            string.Join(",", message.PhoneNumbers),
            message.TemplateCode,
            message.Content?.Length ?? 0);

        return Task.FromResult(SmsSendResult.Success("null"));
    }
}
