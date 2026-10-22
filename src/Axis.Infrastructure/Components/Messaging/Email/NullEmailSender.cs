using Microsoft.Extensions.Logging;

namespace Axis.Infrastructure.Components.Messaging.Email;

/// <summary>
/// 未启用邮件时的空实现：记录调试日志并返回成功，避免开发环境依赖 SMTP。
/// </summary>
public sealed class NullEmailSender(ILogger<NullEmailSender> logger) : IEmailSender
{
    /// <inheritdoc />
    public Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var to = string.Join(", ", message.To.Select(x => x.Address));
        logger.LogDebug(
            "NullEmailSender 跳过发送：To={To} Subject={Subject}",
            to,
            message.Subject);

        return Task.FromResult(EmailSendResult.Success("null"));
    }
}
