using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Axis.Infrastructure.Components.EventBus.Outbox;

/// <summary>
/// Outbox 选项注册。
/// </summary>
public static class OutboxServiceCollectionExtensions
{
    /// <summary>
    /// 绑定 Outbox 配置；传输与 Dispatcher 仍由 IAutoInject 扫描注册。
    /// </summary>
    public static IServiceCollection AddAxisOutbox(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AxisOutboxOptions>(configuration.GetSection(AxisOutboxOptions.SectionName));
        return services;
    }
}
