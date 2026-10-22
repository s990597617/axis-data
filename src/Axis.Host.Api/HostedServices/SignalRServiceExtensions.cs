using Axis.Host.Api.Notify;
using Axis.Infrastructure.Notify;
using Axis.Infrastructure.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Axis.Host.Api.HostedServices;

/// <summary>
/// SignalR 注册与 Hub 映射。
/// </summary>
public static class SignalRServiceExtensions
{
    /// <summary>
    /// 注册 SignalR 与推送实现。须在 <c>AddDependencySetup</c> 之前调用，以便覆盖 NullUserNotifyPusher。
    /// </summary>
    public static IServiceCollection AddAxisSignalR(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(AxisSignalROptions.SectionName);
        services.Configure<AxisSignalROptions>(section);
        var options = section.Get<AxisSignalROptions>() ?? new AxisSignalROptions();
        if (!options.Enabled) return services;

        services.AddSignalR();
        services.AddSingleton<IUserIdProvider, AxisUserIdProvider>();
        // TryAdd：抢先于扫描注册的 Null*
        services.TryAddSingleton<IUserNotifyPusher, SignalRUserNotifyPusher>();
        services.TryAddSingleton<ISessionKickPusher, SignalRSessionKickPusher>();

        // WebSocket 无法自定义 Header，从 query access_token 取 JWT
        services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, jwt =>
        {
            var previous = jwt.Events?.OnMessageReceived;
            jwt.Events ??= new JwtBearerEvents();
            jwt.Events.OnMessageReceived = async context =>
            {
                if (previous is not null)
                    await previous(context);

                if (!string.IsNullOrEmpty(context.Token)) return;

                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken)
                    && path.StartsWithSegments("/hubs", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = accessToken;
                }
            };
        });

        return services;
    }

    /// <summary>
    /// 映射通知 Hub。
    /// </summary>
    public static WebApplication MapAxisSignalRHubs(this WebApplication app, IConfiguration configuration)
    {
        var options = configuration.GetSection(AxisSignalROptions.SectionName).Get<AxisSignalROptions>()
                      ?? new AxisSignalROptions();
        if (!options.Enabled) return app;

        var path = string.IsNullOrWhiteSpace(options.HubPath) ? "/hubs/notify" : options.HubPath;
        app.MapHub<NotifyHub>(path);
        return app;
    }
}
