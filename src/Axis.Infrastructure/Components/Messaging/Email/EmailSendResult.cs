namespace Axis.Infrastructure.Components.Messaging.Email;

/// <summary>
/// 邮件发送结果。
/// </summary>
public sealed class EmailSendResult
{
    /// <summary>
    /// 是否发送成功。
    /// </summary>
    public bool Succeeded { get; init; }

    /// <summary>
    /// 失败原因；成功时为 null。
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 提供方返回的消息标识（如有）。
    /// </summary>
    public string? ProviderMessageId { get; init; }

    /// <summary>
    /// 构造成功结果。
    /// </summary>
    public static EmailSendResult Success(string? providerMessageId = null)
        => new() { Succeeded = true, ProviderMessageId = providerMessageId };

    /// <summary>
    /// 构造失败结果。
    /// </summary>
    public static EmailSendResult Failure(string errorMessage)
        => new() { Succeeded = false, ErrorMessage = errorMessage };
}
