namespace Axis.Infrastructure.Components.Messaging.Email;

/// <summary>
/// 邮件附件。
/// </summary>
public sealed class EmailAttachment
{
    /// <summary>
    /// 附件文件名。
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// MIME Content-Type，默认 <c>application/octet-stream</c>。
    /// </summary>
    public string ContentType { get; init; } = "application/octet-stream";

    /// <summary>
    /// 附件二进制内容。
    /// </summary>
    public required byte[] Content { get; init; }
}
