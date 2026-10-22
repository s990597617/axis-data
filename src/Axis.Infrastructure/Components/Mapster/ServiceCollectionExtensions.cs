using Mapster;
using MapsterMapper;

namespace Axis.Infrastructure.Components.Mapster;

/// <summary>
/// Mapster 服务拓展
/// </summary>
public static class MapsterServiceCollectionExtensions
{
    /// <summary>
    /// 添加对象映射。
    /// 读取 <c>BootstrapAssembly.Mapster</c> 声明的程序集，
    /// 扫描其中的 <see cref="IRegister"/>，并以 <see cref="NameMatchingStrategy.Flexible"/>
    /// （同时包含忽略大小写 + 驼峰/下划线互通）作为全局默认策略。
    /// </summary>
    public static IServiceCollection AddObjectMapper(this IServiceCollection services)
    {
        var config = TypeAdapterConfig.GlobalSettings;

        // 统一程序集加载：失败容错，不阻塞启动
        var assemblies = GlobalAssemblyLoader.Load(GlobalContext.BootstrapAssembly?.Mapster);
        if (assemblies.Length > 0)
        {
            config.Scan(assemblies);
        }

        // 全局默认策略：Flexible 已包含 IgnoreCase + 命名风格互通，不要再二次覆盖
        config.Default
            .NameMatchingStrategy(NameMatchingStrategy.Flexible)
            .PreserveReference(true);

        // 启动期预编译，把映射错误提前到启动而不是首次调用
        try
        {
            config.Compile();
        }
        catch
        {
            // 预编译失败不阻塞启动；运行时调用具体映射时仍会抛原异常便于定位
        }

        // 配置支持依赖注入
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        return services;
    }
}