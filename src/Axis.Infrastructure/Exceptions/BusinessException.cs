using Axis.Infrastructure.Host.Responses;

namespace Axis.Infrastructure.Exceptions;

/// <summary>
/// 业务异常。
/// 由全局异常处理器映射为统一响应体，HTTP 状态码通过 <see cref="ResultCodes.ToHttpStatus(int)"/> 自动推导。
/// <para>业务侧用法：</para>
/// <code>
/// throw new BusinessException("用户名已存在");                          // 默认 42200
/// throw new BusinessException(ResultCodes.NotFound, "订单不存在");       // 40400 → HTTP 404
/// throw new BusinessException(ResultCodes.Conflict, "版本冲突", oldVer); // 40900 → HTTP 409 + 带数据
/// </code>
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// 业务码。默认 <see cref="ResultCodes.BusinessError"/>（42200）。
    /// </summary>
    public int Code { get; }

    /// <summary>
    /// 可选的附加数据，随错误响应的 <c>data</c> 字段返回给前端.
    /// 避免与 <see cref="Exception.Data"/>（IDictionary）冲突，故改名 <c>Payload</c>。
    /// </summary>
    public object Payload { get; }

    /// <summary>
    /// 使用默认业务码构造。
    /// </summary>
    public BusinessException(string message)
        : this(ResultCodes.BusinessError, message, null) { }

    /// <summary>
    /// 使用指定业务码构造。
    /// </summary>
    public BusinessException(int code, string message, object payload = null) : base(message)
    {
        Code = code;
        Payload = payload;
    }

    /// <summary>
    /// 兼容旧 <see cref="ResultCode"/> 枚举构造，新代码请使用 <see cref="ResultCodes"/> 常量。
    /// </summary>
    [Obsolete("请改用 BusinessException(int code, string message)，搭配 ResultCodes 常量。")]
    public BusinessException(ResultCode errorCode, string message) : base(message)
    {
        Code = (int)errorCode switch
        {
            200 => ResultCodes.Success,
            400 => ResultCodes.BusinessError,
            _ => (int)errorCode
        };
    }

    /// <summary>
    /// 兼容旧属性名（旧代码可能读取 <c>ErrorCode</c>）。
    /// </summary>
    [Obsolete("请改用 Code。")]
    public ResultCode ErrorCode => Code == ResultCodes.Success ? ResultCode.Success : ResultCode.Fail;
}