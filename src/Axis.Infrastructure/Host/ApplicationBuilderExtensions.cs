using System.Reflection;
using Axis.Infrastructure.Host.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Axis.Infrastructure.Host;

/// <summary>
/// 应用程序生命周期扩展。
/// <para>
/// 提供 Banner 打印 + 优雅停机两个钩子，
/// 关键差异点：
/// </para>
/// <list type="bullet">
/// <item><description>停机回调内 <b>await</b> 所有 <see cref="IAppShutdownHandler"/>，不再丢 Task（旧实现存在 fire-and-forget bug）</description></item>
/// <item><description>带 30s 超时保护，避免单个 handler 卡死拖慢整体停机</description></item>
/// <item><description>Banner / 停机日志走 <see cref="ILogger"/>，不再裸 <c>Console.WriteLine</c></description></item>
/// </list>
/// </summary>
public static class ApplicationBuilderExtensions
{
    private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(30);

    /// <summary>
    /// 注册启动 Banner 钩子。必须在应用构建完成后调用。
    /// </summary>
    public static IApplicationBuilder UseAxisStartupBanner(this IApplicationBuilder app)
    {
        var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
        var logger = app.ApplicationServices.GetService<ILoggerFactory>()?.CreateLogger("Axis.Kernel.Host")
                     ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        var environment = app.ApplicationServices.GetService<IHostEnvironment>();

        lifetime.ApplicationStarted.Register(() =>
            WriteBanner(logger, environment?.EnvironmentName, ResolveHostVersion()));
        return app;
    }

    /// <summary>
    /// 注册优雅停机钩子。解析所有 <see cref="IAppShutdownHandler"/> 并依次 await；
    /// 超过 <see cref="ShutdownTimeout"/> 后强制继续。
    /// </summary>
    public static IApplicationBuilder UseAxisShutdownHandler(this IApplicationBuilder app)
    {
        var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
        var logger = app.ApplicationServices.GetService<ILoggerFactory>()?.CreateLogger("Axis.Kernel.Host")
                     ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

        lifetime.ApplicationStopping.Register(() =>
        {
            using var cts = new CancellationTokenSource(ShutdownTimeout);
            var handlers = app.ApplicationServices.GetServices<IAppShutdownHandler>().ToArray();
            if (handlers.Length == 0) return;

            logger.LogInformation("开始执行 {Count} 个关机处理器...", handlers.Length);
            try
            {
                Task.Run(async () =>
                {
                    foreach (var handler in handlers)
                    {
                        try { await handler.ShutdownAsync().WaitAsync(cts.Token); }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "关机处理器执行失败：{Handler}", handler.GetType().FullName);
                        }
                    }
                }, cts.Token).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("关机处理器在 {Timeout}s 内未完成，已强制终止。", ShutdownTimeout.TotalSeconds);
            }
        });

        lifetime.ApplicationStopped.Register(() => logger.LogInformation("Axis 应用已完全停止。"));
        return app;
    }

    /// <summary>
    /// 将启动 Banner 与停机钩子一次性注册。
    /// </summary>
    public static IApplicationBuilder UseAxisLifetime(this IApplicationBuilder app)
        => app.UseAxisStartupBanner().UseAxisShutdownHandler();

    private static void WriteBanner(ILogger logger, string? environmentName, string version)
    {
        var env = string.IsNullOrWhiteSpace(environmentName) ? "Production" : environmentName.Trim();
        var startedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // 纯 ASCII 字标：去掉扑克牌风格与大片空框；信息区做成启动铭牌
        var banner =
            $"""

            =================================================================
                                  _    __  _____  ____
                                 / \   \ \/ /_ _|/ ___|
                                / _ \   \  / | | \___ \
                               / ___ \  /  \ | |  ___) |
                              /_/   \_\/_/\_\___||____/

                              Axis  ·  云枢 MES
            -----------------------------------------------------------------
              Environment : {env,-18} Version : {version}
              Started At  : {startedAt,-18} Status  : Ready
            =================================================================
              Startup complete. Welcome to Axis MES.
            """;

        logger.LogInformation("{Banner}", banner);
    }

    private static string ResolveHostVersion()
    {
        var entry = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var informational = entry.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
        {
            // 去掉 SourceRevisionId 等后缀，只留主版本展示
            var plus = informational.IndexOf('+');
            return plus > 0 ? informational[..plus] : informational;
        }

        var version = entry.GetName().Version;
        return version is null ? "dev" : $"{version.Major}.{version.Minor}.{version.Build}";
    }
}
