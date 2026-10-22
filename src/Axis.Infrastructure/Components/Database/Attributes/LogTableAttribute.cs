namespace Axis.Infrastructure.Components.Database.Attributes;

/// <summary>
/// 日志库表特性：仓储将路由到 <c>log</c> 连接（若已配置）。
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class LogTableAttribute : Attribute
{
}
