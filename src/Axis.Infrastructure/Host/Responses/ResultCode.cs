using System.ComponentModel;

namespace Axis.Infrastructure.Host.Responses;

/// <summary>
/// 返回结果编码
/// </summary>
public enum ResultCode
{
    /// <summary>
    /// 操作失败
    /// </summary>
    [Description("操作失败！")]
    Fail = 400,

    /// <summary>
    /// 请求成功
    /// </summary>
    [Description("请求成功！")]
    Success = 200
}