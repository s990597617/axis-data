namespace Axis.Infrastructure.Components.Messaging.Sms;

/// <summary>
/// 短信发送端口。
/// </summary>
/// <remarks>
/// 厂商差异（阿里云 / 腾讯云等）收敛在实现类；业务只传手机号、模板与变量。
/// 未启用或 Provider=None 时注册空实现。
/// </remarks>
public interface ISmsSender
{
    /// <summary>
    /// 发送短信。
    /// </summary>
    /// <param name="message">短信内容。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>发送结果。</returns>
    Task<SmsSendResult> SendAsync(SmsMessage message, CancellationToken ct = default);
}
