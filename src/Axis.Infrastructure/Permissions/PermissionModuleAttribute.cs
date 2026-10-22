namespace Axis.Infrastructure.Permissions;

/// <summary>
/// 标记权限码嵌套类所属业务模块，供权限目录扫描与前端级联选择使用。
/// </summary>
/// <param name="key">模块键，如 <c>config</c>；对应权限码中段。</param>
/// <param name="name">模块中文名，如「参数配置」。</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PermissionModuleAttribute(string key, string name) : Attribute
{
    /// <summary>
    /// 模块键（英文短码）。
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    /// 模块显示名称。
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 目录排序；数值越小越靠前。
    /// </summary>
    public int Order { get; init; }
}
