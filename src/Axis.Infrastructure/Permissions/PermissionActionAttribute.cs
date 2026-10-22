namespace Axis.Infrastructure.Permissions;

/// <summary>
/// 标记权限码常量对应的动作显示名，供权限目录与菜单按钮表单预填标题。
/// </summary>
/// <param name="name">动作中文名，如「查询」「新增」。</param>
[AttributeUsage(AttributeTargets.Field, Inherited = false)]
public sealed class PermissionActionAttribute(string name) : Attribute
{
    /// <summary>
    /// 动作显示名称。
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 模块内排序；数值越小越靠前。
    /// </summary>
    public int Order { get; init; }
}
