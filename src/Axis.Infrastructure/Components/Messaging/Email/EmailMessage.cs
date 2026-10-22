namespace Axis.Infrastructure.Components.Messaging.Email;

/// <summary>
/// 待发送邮件。
/// </summary>
public sealed class EmailMessage
{
    /// <summary>
    /// 发件人；为空时使用配置中的默认发件人。
    /// </summary>
    public EmailAddress? From { get; init; }

    /// <summary>
    /// 收件人（至少一个）。
    /// </summary>
    public required IReadOnlyList<EmailAddress> To { get; init; }

    /// <summary>
    /// 抄送；可空。
    /// </summary>
    public IReadOnlyList<EmailAddress>? Cc { get; init; }

    /// <summary>
    /// 密送；可空。
    /// </summary>
    public IReadOnlyList<EmailAddress>? Bcc { get; init; }

    /// <summary>
    /// 主题。
    /// </summary>
    public required string Subject { get; init; }

    /// <summary>
    /// 正文。
    /// </summary>
    public required string Body { get; init; }

    /// <summary>
    /// 正文是否为 HTML。
    /// </summary>
    public bool IsHtml { get; init; }

    /// <summary>
    /// 附件列表；可空。
    /// </summary>
    public IReadOnlyList<EmailAttachment>? Attachments { get; init; }
}
