namespace Axis.Infrastructure.Components.Messaging.Email;

/// <summary>
/// 邮件发送端口。
/// </summary>
/// <remarks>
/// Application / Handler 只依赖本接口；SMTP 细节由 Infrastructure 实现。
/// 未启用邮件时注册空实现，保证业务可注入且开发环境不依赖真实邮箱。
/// </remarks>
public interface IEmailSender
{
    /// <summary>
    /// 发送一封邮件。
    /// </summary>
    /// <param name="message">邮件内容。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>发送结果（不抛业务可预期失败，由 <see cref="EmailSendResult.Succeeded"/> 表达）。</returns>
    Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken ct = default);
}
