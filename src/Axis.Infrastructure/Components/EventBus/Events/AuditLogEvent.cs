namespace Axis.Infrastructure.Components.EventBus.Events;

/// <summary>
/// 审计日志类型
/// </summary>
public enum AuditLogType
{
    /// <summary>
    /// 操作日志
    /// </summary>
    Operate,

    /// <summary>
    /// 访问日志
    /// </summary>
    Visit,

    /// <summary>
    /// 异常日志
    /// </summary>
    Error
}

/// <summary>
/// 审计日志事件（由中间件自动发布）。
/// <para>
/// 合并原来的 ApplicationOpCommand / ApplicationVisCommand / ApplicationErrorCommand 为统一事件，
/// 通过 <see cref="LogType"/> 区分类型，由不同的 Handler 分别入库。
/// </para>
/// </summary>
public class AuditLogEvent : AxisEvent
{
    /// <summary>
    /// 日志类型
    /// </summary>
    public AuditLogType LogType { get; init; }

    /// <summary>
    /// 操作人 ID（从 JWT 解析，未登录为 null）
    /// </summary>
    public long? UserId { get; init; }

    /// <summary>
    /// 操作人账号
    /// </summary>
    public string Account { get; init; }

    /// <summary>
    /// 端点名称/功能描述
    /// </summary>
    public string DisplayTitle { get; init; }

    /// <summary>
    /// 请求方法全名
    /// </summary>
    public string FullName { get; init; }

    /// <summary>
    /// 请求方法名称
    /// </summary>
    public string ActionName { get; init; }

    /// <summary>
    /// 请求方式
    /// </summary>
    public string HttpMethod { get; init; }

    /// <summary>
    /// 请求地址
    /// </summary>
    public string RequestUrl { get; init; }

    /// <summary>
    /// 请求参数
    /// </summary>
    public string RequestParam { get; init; }

    /// <summary>
    /// IP 地址
    /// </summary>
    public string RemoteIp { get; init; }

    /// <summary>
    /// IP 端口
    /// </summary>
    public int RemotePort { get; init; }

    /// <summary>
    /// User-Agent
    /// </summary>
    public string UserAgent { get; init; }

    /// <summary>
    /// 操作用时（毫秒）
    /// </summary>
    public long Elapsed { get; init; }

    /// <summary>
    /// 状态码
    /// </summary>
    public string State { get; init; }

    /// <summary>
    /// 跟踪 ID
    /// </summary>
    public string TraceId { get; init; }

    /// <summary>
    /// 线程 ID
    /// </summary>
    public int ThreadId { get; init; }

    /// <summary>
    /// 异常信息（仅 Error 类型有值）
    /// </summary>
    public string Exception { get; init; }
}
