namespace Axis.Infrastructure.Options;

/// <summary>
/// 邮件发送配置（SMTP / MailKit）。
/// </summary>
public sealed class AxisEmailOptions
{
    /// <summary>
    /// 配置节名称。
    /// </summary>
    public const string SectionName = "Email";

    /// <summary>
    /// 是否启用真实 SMTP 发送；关闭时注册空实现，业务可照常注入 <c>IEmailSender</c>。
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// SMTP 主机。
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// SMTP 端口（常见 465 / 587 / 25）。
    /// </summary>
    public int Port { get; set; } = 465;

    /// <summary>
    /// 登录账号；部分服务商要求完整邮箱。
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 登录密码或授权码。
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// 默认发件人地址。
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// 默认发件人显示名。
    /// </summary>
    public string FromDisplayName { get; set; } = "Axis";

    /// <summary>
    /// 是否使用 SSL/TLS（465 通常为 true；587 也可先 Connect 再 StartTls）。
    /// </summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>
    /// 发送超时（秒）。
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
