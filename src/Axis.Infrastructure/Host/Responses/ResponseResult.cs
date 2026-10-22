namespace Axis.Infrastructure.Host.Responses;

/// <summary>
/// 全局统一响应体。
/// <para>
/// 与前端约定的 JSON schema（字段按 <c>PropertyNamingPolicy.CamelCase</c> 序列化）：
/// </para>
/// <code>
/// {
///   "code": 10000,                                // 业务码，详见 ResultCodes；前 3 位 == HTTP status
///   "success": true,                              // 便捷成功标识，== (code == 10000)
///   "message": "",                                // 成功时通常为空；失败时为错误原因摘要。
///   "data": { ... },                              // 业务数据；错误时为 null
///   "traceId": "8b4a3f...",                       // 链路追踪 ID，与服务端日志对应
///   "path": "/api/users/1",                       // 请求路径
///   "timestamp": "2026-04-23T15:17:12.345+08:00"  // ISO 8601，服务器本地时间
/// }
/// </code>
/// </summary>
public class ResponseResult<T>
{
    /// <summary>
    /// 业务码。取值参见 <see cref="ResultCodes"/>；前 3 位等于 HTTP 状态码。
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 是否成功。等价于 <c>Code == <see cref="ResultCodes.Success"/></c>，前端可直接使用。
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 成功时通常为空；失败时为错误原因摘要。
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 业务数据载荷。错误时为 <c>null</c>。
    /// </summary>
    public T Data { get; set; }

    /// <summary>
    /// 链路追踪 ID，对应服务端日志的 TraceId，方便前后端联调排错。
    /// </summary>
    public string TraceId { get; set; }

    /// <summary>
    /// 请求路径，便于前端上报 / 埋点。
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// 响应时间（服务器本地时间，ISO 8601）。
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
}

/// <summary>
/// <see cref="ResponseResult{T}"/> 的便捷工厂。
/// FastEndpoints 场景下响应体由 <c>ResponseSerializer</c> 自动包装，无需手动调用；
/// 非 FastEndpoints 端点 / 异常处理器 / 中间件可用本工厂直接构造。
/// </summary>
public static class ResponseResult
{
    /// <summary>构造成功响应。</summary>
    public static ResponseResult<T> Ok<T>(T data, string message = "") => new()
    {
        Code = ResultCodes.Success,
        Success = true,
        Message = message,
        Data = data,
        Timestamp = DateTimeOffset.Now
    };

    /// <summary>构造失败响应。</summary>
    public static ResponseResult<object> Fail(int code, string message, object data = null) => new()
    {
        Code = code,
        Success = false,
        Message = message,
        Data = data,
        Timestamp = DateTimeOffset.Now
    };
}
