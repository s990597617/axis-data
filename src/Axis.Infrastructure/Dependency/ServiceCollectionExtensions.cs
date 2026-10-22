using System.Reflection;
using Axis.Infrastructure.Components.Database.Permission;
using Axis.Infrastructure.Dependency.Annotations;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Axis.Infrastructure.Dependency;

/// <summary>
/// 依赖服务注册器拓展类
/// </summary>
public static class DependencyServiceCollectionExtensions
{
    /// <summary>
    /// 框架内部标记接口集合，注册为服务时应被过滤掉，避免污染 DI 清单。
    /// </summary>
    private static readonly HashSet<Type> FrameworkMarkerInterfaces = [typeof(IAutoInject)];

    /// <summary>
    /// 允许多实现并存的接口（供 <c>IEnumerable&lt;T&gt;</c> 注入）；不能用 TryAdd。
    /// </summary>
    private static readonly HashSet<Type> MultiRegistrationInterfaces =
    [
        typeof(IDataPermissionContributor),
        typeof(Jobs.IAxisJob)
    ];

    /// <summary>
    /// 扫描宿主输出目录下所有 Axis.* 程序集，将实现了 <see cref="IAutoInject"/> 的类注册到容器。
    /// 生命周期与注册方式通过 <see cref="InjectionAttribute"/> 声明，缺省为 Scoped + 按接口注册。
    /// </summary>
    /// <param name="services">服务集合</param>
    public static IServiceCollection AddDependencySetup(this IServiceCollection services)
    {
        // 扫描宿主输出目录下所有 Axis.* 程序集（无需再在配置中维护白名单）
        var businessTypes = GlobalAssemblyLoader.LoadAxisTypes();
        var candidates = businessTypes
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false }
                        && typeof(IAutoInject).IsAssignableFrom(t))
            .Distinct()
            .ToArray();

        foreach (var implType in candidates)
        {
            var attr = implType.GetCustomAttribute<InjectionAttribute>() ?? new InjectionAttribute();

            switch (attr.As)
            {
                case InjectAs.Self:
                    services.TryAdd(new ServiceDescriptor(implType, implType, attr.Lifetime));
                    break;

                case InjectAs.ImplementedInterfaces:
                    RegisterInterfaces(services, implType, attr.Lifetime);
                    break;

                case InjectAs.SelfWithInterfaces:
                    services.TryAdd(new ServiceDescriptor(implType, implType, attr.Lifetime));
                    RegisterInterfaces(services, implType, attr.Lifetime, forwardToSelf: true);
                    break;
            }
        }

        return services;
    }

    /// <summary>
    /// 按实现接口注册，过滤框架标记接口；对同一服务类型采用 Skip 策略避免重复注册。
    /// </summary>
    private static void RegisterInterfaces(IServiceCollection services, Type implType, ServiceLifetime lifetime, bool forwardToSelf = false)
    {
        var interfaces = implType.GetInterfaces()
            .Where(i => !FrameworkMarkerInterfaces.Contains(i))
            .Where(i => i.Namespace is null || !i.Namespace.StartsWith("Axis.Infrastructure.Dependency.Annotations", StringComparison.Ordinal))
            .ToArray();

        if (interfaces.Length == 0) return;

        foreach (var serviceType in interfaces)
        {
            // 转发到自身，避免单例场景下同一实现被多次实例化
            var descriptor =
                forwardToSelf
                    ? new ServiceDescriptor(serviceType, sp => sp.GetRequiredService(implType), lifetime)
                    : new ServiceDescriptor(serviceType, implType, lifetime);

            if (MultiRegistrationInterfaces.Contains(serviceType))
                services.Add(descriptor);
            else
                services.TryAdd(descriptor);
        }
    }
}