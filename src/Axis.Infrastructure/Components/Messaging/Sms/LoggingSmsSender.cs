using Axis.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Axis.Infrastructure.Components.Messaging.Sms;

/// <summary>
/// 开发用短信实现：仅写日志，不调用外部网关。
/// </summary>
/// <remarks>
/// 配置 <c>Sms:Provider=Logging</c> 且 <c>Enabled=true</c> 时使用，便于联调验证码流程。
/// </remarks>
public sealed class LoggingSmsSender(
    IOptions<AxisSmsOptions> options,
    ILogger<LoggingSmsSender> logger) : ISmsSender
{
    /// <inheritdoc />
    public Task<SmsSendResult> SendAsync(SmsMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.PhoneNumbers is null || message.PhoneNumbers.Count == 0)
            return Task.FromResult(SmsSendResult.Failure("手机号不能为空"));

        var sign = string.IsNullOrWhiteSpace(message.SignName)
            ? options.Value.DefaultSignName
            : message.SignName;

        logger.LogInformation(
            "LoggingSmsSender：Sign={Sign} Phones={Phones} Template={Template} Params={Params} Content={Content}",
            sign,
            string.Join(",", message.PhoneNumbers),
            message.TemplateCode,
            message.TemplateParams is null
                ? null
                : string.Join(",", message.TemplateParams.Select(kv => $"{kv.Key}={kv.Value}")),
            message.Content);

        return Task.FromResult(SmsSendResult.Success($"log-{Guid.NewGuid():N}"));
    }
}
