namespace Axis.Infrastructure.Attributes;

/// <summary>
/// 操作日志
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class OperateLogAttribute : Attribute
{

}