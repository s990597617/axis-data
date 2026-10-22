namespace Axis.Infrastructure.Dependency.Annotations;

/// <summary>
/// 注册方式
/// </summary>
public enum InjectAs
{
    /// <summary>
    /// 按实现的接口注册（默认）。
    /// </summary>
    ImplementedInterfaces = 0,

    /// <summary>
    /// 按自身类型注册。
    /// </summary>
    Self = 1,

    /// <summary>
    /// 同时按自身与实现的接口注册。
    /// </summary>
    SelfWithInterfaces = 2,
}

/// <summary>
/// 标记一个实现了 <see cref="IAutoInject"/> 的类如何被自动注入到 DI 容器。
/// 未显式标注时，默认为 <see cref="ServiceLifetime.Scoped"/> + <see cref="InjectAs.ImplementedInterfaces"/>。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class InjectionAttribute : Attribute
{
    /// <summary>
    /// 服务生命周期。
    /// </summary>
    public ServiceLifetime Lifetime { get; }

    /// <summary>
    /// 注册方式。
    /// </summary>
    public InjectAs As { get; }

    /// <summary>
    /// 构造注入特性。
    /// </summary>
    /// <param name="lifetime">服务生命周期，默认 Scoped</param>
    /// <param name="as">注册方式，默认按接口</param>
    public InjectionAttribute(ServiceLifetime lifetime = ServiceLifetime.Scoped, InjectAs @as = InjectAs.ImplementedInterfaces)
    {
        Lifetime = lifetime;
        As = @as;
    }
}
