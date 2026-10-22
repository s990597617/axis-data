namespace Axis.Infrastructure.Components.EventBus.Outbox;

/// <summary>
/// 扫描已加载 Axis 程序集中的 <see cref="IIntegrationEvent"/> 实现。
/// </summary>
[Injection(ServiceLifetime.Singleton)]
public sealed class IntegrationEventTypeMap : IIntegrationEventTypeMap, IAutoInject
{
    private readonly Dictionary<string, Type> _map;

    /// <summary>
    /// 构建类型目录。
    /// </summary>
    public IntegrationEventTypeMap()
    {
        _map = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => assembly.GetName().Name?.StartsWith("Axis.", StringComparison.Ordinal) == true)
            .SelectMany(SafeGetTypes)
            .Where(type => type is { IsClass: true, IsAbstract: false }
                           && typeof(IIntegrationEvent).IsAssignableFrom(type)
                           && type.FullName is not null)
            .ToDictionary(type => type.FullName!, type => type, StringComparer.Ordinal);
    }

    /// <summary>
    /// 按全名解析。
    /// </summary>
    public Type? Resolve(string eventType)
        => string.IsNullOrWhiteSpace(eventType) ? null : _map.GetValueOrDefault(eventType);

    private static IEnumerable<Type> SafeGetTypes(System.Reflection.Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (System.Reflection.ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type is not null)!;
        }
    }
}
