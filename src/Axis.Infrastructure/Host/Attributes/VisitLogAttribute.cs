namespace Axis.Infrastructure.Attributes;

/// <summary>
/// 访问日志
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class VisitLogAttribute : Attribute
{

}