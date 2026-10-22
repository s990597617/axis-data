using Axis.Infrastructure.Components.Messaging.Email;
using Axis.Infrastructure.Components.Messaging.Sms;
using Axis.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Axis.Infrastructure.Components.Messaging;

/// <summary>
/// 邮件 / 短信消息通道注册扩展。
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// 注册邮件与短信发送端口。
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description><c>Email:Enabled=false</c> → <see cref="NullEmailSender"/>。</description></item>
    ///   <item><description><c>Email:Enabled=true</c> 且 Host 为空 → <see cref="LoggingEmailSender"/>（联调）。</description></item>
    ///   <item><description><c>Email:Enabled=true</c> 且配置 Host → <see cref="MailKitSmtpEmailSender"/>。</description></item>
    ///   <item><description><c>Sms:Enabled=true</c> 且 Provider=<c>Logging</c> → <see cref="LoggingSmsSender"/>；其它未识别 Provider 回退空实现。</description></item>
    /// </list>
    /// 阿里云 / 腾讯云等厂商实现可在 Host 的 <c>serviceAction</c> 中覆盖 <see cref="ISmsSender"/>。
    /// </remarks>
    /// <param name="services">服务集合。</param>
    /// <param name="configuration">配置根。</param>
    public static IServiceCollection AddAxisMessaging(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AxisEmailOptions>(configuration.GetSection(AxisEmailOptions.SectionName));
        services.Configure<AxisSmsOptions>(configuration.GetSection(AxisSmsOptions.SectionName));

        var email = configuration.GetSection(AxisEmailOptions.SectionName).Get<AxisEmailOptions>()
                    ?? new AxisEmailOptions();
        var sms = configuration.GetSection(AxisSmsOptions.SectionName).Get<AxisSmsOptions>()
                  ?? new AxisSmsOptions();

        if (!email.Enabled)
            services.TryAddSingleton<IEmailSender, NullEmailSender>();
        else if (string.IsNullOrWhiteSpace(email.Host))
            services.TryAddSingleton<IEmailSender, LoggingEmailSender>();
        else
            services.TryAddSingleton<IEmailSender, MailKitSmtpEmailSender>();

        if (sms.Enabled && IsLoggingProvider(sms.Provider))
            services.TryAddSingleton<ISmsSender, LoggingSmsSender>();
        else
            services.TryAddSingleton<ISmsSender, NullSmsSender>();

        return services;
    }

    /// <summary>
    /// 覆盖注册自定义短信实现（如阿里云 / 腾讯云）。
    /// </summary>
    public static IServiceCollection AddSmsSender<TImpl>(this IServiceCollection services)
        where TImpl : class, ISmsSender
    {
        services.RemoveAll<ISmsSender>();
        services.AddSingleton<ISmsSender, TImpl>();
        return services;
    }

    /// <summary>
    /// 覆盖注册自定义邮件实现。
    /// </summary>
    public static IServiceCollection AddEmailSender<TImpl>(this IServiceCollection services)
        where TImpl : class, IEmailSender
    {
        services.RemoveAll<IEmailSender>();
        services.AddSingleton<IEmailSender, TImpl>();
        return services;
    }

    private static bool IsLoggingProvider(string? provider)
        => string.Equals(provider, "Logging", StringComparison.OrdinalIgnoreCase)
           || string.Equals(provider, "Log", StringComparison.OrdinalIgnoreCase);
}
