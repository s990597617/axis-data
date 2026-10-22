namespace Axis.Infrastructure.Components.Database.Attributes;

/// <summary>
/// 系统表特性：仓储固定使用主库 master，不随独立库租户切换。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class SysTableAttribute : Attribute
{
}
