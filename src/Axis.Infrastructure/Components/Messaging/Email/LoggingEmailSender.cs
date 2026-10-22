using Microsoft.Extensions.Logging;

namespace Axis.Infrastructure.Components.Messaging.Email;

/// <summary>
/// 开发用邮件实现：仅写日志，不连接 SMTP。
/// </summary>
/// <remarks>
/// 在 <c>Email:Enabled=true</c> 且未配置 Host 时自动注册，便于联调通知通道。
/// </remarks>
public sealed class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    /// <inheritdoc />
    public Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.To is null || message.To.Count == 0)
            return Task.FromResult(EmailSendResult.Failure("收件人不能为空"));

        logger.LogInformation(
            "LoggingEmailSender：To={To} Subject={Subject} IsHtml={IsHtml} BodyLength={Length}",
            string.Join(", ", message.To.Select(x => x.Address)),
            message.Subject,
            message.IsHtml,
            message.Body?.Length ?? 0);

        return Task.FromResult(EmailSendResult.Success($"log-{Guid.NewGuid():N}"));
    }
}
