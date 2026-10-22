using Axis.Infrastructure.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Axis.Infrastructure.Components.Messaging.Email;

/// <summary>
/// 基于 MailKit 的 SMTP 邮件发送实现。
/// </summary>
public sealed class MailKitSmtpEmailSender(
    IOptions<AxisEmailOptions> options,
    ILogger<MailKitSmtpEmailSender> logger) : IEmailSender
{
    /// <inheritdoc />
    public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (message.To is null || message.To.Count == 0)
            return EmailSendResult.Failure("收件人不能为空");

        var cfg = options.Value;
        if (string.IsNullOrWhiteSpace(cfg.Host))
            return EmailSendResult.Failure("未配置 Email:Host");

        var fromAddress = message.From?.Address ?? cfg.FromAddress;
        if (string.IsNullOrWhiteSpace(fromAddress))
            return EmailSendResult.Failure("未指定发件人，且 Email:FromAddress 为空");

        try
        {
            using var mime = BuildMimeMessage(message, cfg, fromAddress);
            using var client = new SmtpClient();
            client.Timeout = Math.Max(1, cfg.TimeoutSeconds) * 1000;

            var secure = ResolveSecureOptions(cfg);
            await client.ConnectAsync(cfg.Host, cfg.Port, secure, ct).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(cfg.UserName))
            {
                await client.AuthenticateAsync(cfg.UserName, cfg.Password ?? string.Empty, ct)
                    .ConfigureAwait(false);
            }

            var response = await client.SendAsync(mime, ct).ConfigureAwait(false);
            await client.DisconnectAsync(true, ct).ConfigureAwait(false);

            logger.LogInformation(
                "邮件已发送：To={To} Subject={Subject}",
                string.Join(", ", message.To.Select(x => x.Address)),
                message.Subject);

            return EmailSendResult.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "邮件发送失败：Subject={Subject}", message.Subject);
            return EmailSendResult.Failure(ex.Message);
        }
    }

    private static MimeMessage BuildMimeMessage(
        EmailMessage message,
        AxisEmailOptions cfg,
        string fromAddress)
    {
        var mime = new MimeMessage();
        var fromDisplay = message.From?.DisplayName ?? cfg.FromDisplayName;
        mime.From.Add(string.IsNullOrWhiteSpace(fromDisplay)
            ? MailboxAddress.Parse(fromAddress)
            : new MailboxAddress(fromDisplay, fromAddress));

        foreach (var to in message.To)
            mime.To.Add(ToMailbox(to));

        if (message.Cc is { Count: > 0 })
        {
            foreach (var cc in message.Cc)
                mime.Cc.Add(ToMailbox(cc));
        }

        if (message.Bcc is { Count: > 0 })
        {
            foreach (var bcc in message.Bcc)
                mime.Bcc.Add(ToMailbox(bcc));
        }

        mime.Subject = message.Subject ?? string.Empty;

        var bodyBuilder = new BodyBuilder();
        if (message.IsHtml)
            bodyBuilder.HtmlBody = message.Body;
        else
            bodyBuilder.TextBody = message.Body;

        if (message.Attachments is { Count: > 0 })
        {
            foreach (var attachment in message.Attachments)
            {
                bodyBuilder.Attachments.Add(
                    attachment.FileName,
                    attachment.Content,
                    ContentType.Parse(attachment.ContentType));
            }
        }

        mime.Body = bodyBuilder.ToMessageBody();
        return mime;
    }

    private static MailboxAddress ToMailbox(EmailAddress address)
        => string.IsNullOrWhiteSpace(address.DisplayName)
            ? MailboxAddress.Parse(address.Address)
            : new MailboxAddress(address.DisplayName, address.Address);

    private static SecureSocketOptions ResolveSecureOptions(AxisEmailOptions cfg)
    {
        if (!cfg.UseSsl)
            return SecureSocketOptions.StartTlsWhenAvailable;

        // 465 常见 Implicit SSL；其它端口优先 StartTls。
        return cfg.Port == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;
    }
}
