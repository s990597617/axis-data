namespace Axis.Infrastructure.Host.Tools;

/// <summary>
/// Axis 宿主命令行工具标识。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class HostToolAttribute : Attribute
{
    /// <summary>
    /// 初始化宿主工具特性。
    /// </summary>
    /// <param name="name">工具命令名称。</param>
    /// <param name="description">工具描述。</param>
    public HostToolAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }

    /// <summary>
    /// 工具命令名称。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 工具描述。
    /// </summary>
    public string Description { get; }
}