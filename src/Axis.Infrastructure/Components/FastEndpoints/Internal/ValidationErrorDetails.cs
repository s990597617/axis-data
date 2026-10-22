namespace Axis.Infrastructure.Components.FastEndpoints.Internal;

/// <summary>
/// 验证异常类
/// </summary>
public class ValidationErrorDetails
{
    /// <summary>
    /// 异常状态
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 应用程序路径
    /// </summary>
    public string Instance { get; set; }

    /// <summary>
    /// 异常消息
    /// </summary>
    public string ErrorMessage { get; set; }
}