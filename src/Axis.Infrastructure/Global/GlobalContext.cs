using Axis.Infrastructure.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Axis.Infrastructure.Global;

/// <summary>
/// Axis Framework 全局上下文基座类。
/// 用于统一管理：
/// - 配置（Configuration）
/// - DI 容器（ServiceCollection / RootServices）
/// - 环境信息（Environment）
/// - Web 管道（App）
///
/// 本类分为两个初始化阶段：
/// 1. Bootstrap 阶段（仅从配置读取，不能热更新）
/// 2. Runtime 阶段（应用构建完成，可解析服务）
///
/// 注意：本类作为全局静态上下文，不建议滥用，用于框架级公共组件。
/// </summary>
public static class GlobalContext
{
    /// <summary>
    /// DI 容器的根 ServiceProvider（应用构建完成后可用）。
    /// </summary>
    public static IServiceProvider RootServices { get; private set; }

    /// <summary>
    /// 全局的服务集合（注册阶段可用）。
    /// </summary>
    public static IServiceCollection ServiceCollection { get; private set; }

    /// <summary>
    /// 全局配置对象（可用于绑定 Options）。
    /// </summary>
    public static IConfiguration Configuration { get; private set; }

    /// <summary>
    /// ASP.NET Core 环境信息（如 Development、Production）。
    /// </summary>
    // public static IWebHostEnvironment Environment { get; private set; }

    /// <summary>
    /// 应用管道构建器（可用于注册中间件）。
    /// </summary>
    public static IApplicationBuilder App { get; private set; }

    /// <summary>
    /// Scope 工厂，用于创建作用域解析 Scoped 服务。
    /// </summary>
    private static IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// JWT 配置（Jwt 节）。
    /// </summary>
    public static AxisJwtOptions BootstrapJwt { get; private set; }

    /// <summary>
    /// 系统基础配置（System 节）。
    /// </summary>
    public static AxisSystemOptions BootstrapSystem { get; private set; }

    /// <summary>
    /// 上传配置（Upload 节）。
    /// </summary>
    public static AxisUploadOptions BootstrapUpload { get; private set; }

    /// <summary>
    /// Swagger 配置（Swagger 节）。
    /// </summary>
    public static AxisSwaggerOptions BootstrapSwagger { get; private set; }

    /// <summary>
    /// 代码生成配置（CodeGen 节）。
    /// </summary>
    public static AxisCodeGenOptions BootstrapCodeGen { get; private set; }

    /// <summary>
    /// 程序集加载配置（Assembly 节）。
    /// </summary>
    public static AxisAssemblyOptions BootstrapAssembly { get; private set; }

    /// <summary>
    /// 数据库连接配置（DbConnection 节）。
    /// </summary>
    public static AxisDbConnectionOptions BootstrapDb { get; private set; }

    /// <summary>
    /// 框架缓存配置（AxisCaching 节）。
    /// </summary>
    public static AxisCachingOptions BootstrapCaching { get; private set; }

    /// <summary>
    /// 初始化 Bootstrap 阶段。
    /// </summary>
    /// <param name="config">IConfiguration 对象</param>
    public static void InitBootstrap(IConfiguration config)
    {
        Configuration = config;

        BootstrapJwt = Bind<AxisJwtOptions>("Jwt");
        BootstrapSystem = Bind<AxisSystemOptions>("System");
        BootstrapUpload = Bind<AxisUploadOptions>("Upload");
        BootstrapSwagger = Bind<AxisSwaggerOptions>("Swagger");
        BootstrapCodeGen = Bind<AxisCodeGenOptions>("CodeGen");
        BootstrapAssembly = Bind<AxisAssemblyOptions>("Assembly");
        BootstrapDb = Bind<AxisDbConnectionOptions>("DbConnection");
        BootstrapCaching = Bind<AxisCachingOptions>("AxisCaching");
    }

    /// <summary>
    /// 初始化 Runtime 阶段。
    /// 在 WebApplication 构建完成后调用，可用于解析服务与访问应用环境。
    /// </summary>
    public static void InitRuntime(
        IServiceProvider provider,
        IServiceCollection services,
        IApplicationBuilder app)
    {
        RootServices = provider;
        ServiceCollection = services;
        App = app;
        _scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
    }

    /// <summary>
    /// 从配置节绑定 Options 对象。
    /// 如果配置不存在或格式错误则抛出异常。
    /// </summary>
    private static T Bind<T>(string section) where T : class
    {
        var obj = Configuration.GetSection(section).Get<T>();
        return obj ?? throw new ApplicationException($"{section} 配置为空或格式错误！");
    }

    /// <summary>
    /// 解析一个已注册的服务（Singleton / Transient）。
    /// </summary>
    public static T GetService<T>() => RootServices.GetRequiredService<T>();

    /// <summary>
    /// 解析一个已注册的服务，类型在运行期传入。
    /// </summary>
    public static object GetService(Type t) => RootServices.GetRequiredService(t);

    /// <summary>
    /// 创建一个新的 Scope（用于手动管理 Scoped 生命周期）。
    /// </summary>
    public static IServiceScope CreateScope() => _scopeFactory.CreateScope();

    /// <summary>
    /// 快速解析一个 Scoped 服务。
    /// 自动创建并释放 Scope，仅适用于短生命周期的操作。
    /// </summary>
    public static T GetScopedService<T>()
    {
        using var scope = _scopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }
}
