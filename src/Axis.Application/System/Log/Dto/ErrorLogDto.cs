namespace Axis.Application.System.Log.Dto;

/// <summary>
/// 异常日志列表项。
/// </summary>
public sealed class ErrorLogDto
{
    /// <summary>
    /// 主键。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 功能描述；可为空。
    /// </summary>
    public string? DisplayTitle { get; set; }

    /// <summary>
    /// 方法全名；可为空。
    /// </summary>
    public string? FullName { get; set; }

    /// <summary>
    /// 方法名称；可为空。
    /// </summary>
    public string? ActionName { get; set; }

    /// <summary>
    /// HTTP 方法；可为空。
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// 请求地址；可为空。
    /// </summary>
    public string? RequestUrl { get; set; }

    /// <summary>
    /// 请求参数；可为空。
    /// </summary>
    public string? RequestParam { get; set; }

    /// <summary>
    /// 客户端 IP；可为空。
    /// </summary>
    public string? RemoteIp { get; set; }

    /// <summary>
    /// User-Agent；可为空。
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 耗时（毫秒）。
    /// </summary>
    public long Elapsed { get; set; }

    /// <summary>
    /// 异常信息；可为空。
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// 跟踪 Id；可为空。
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// 线程 Id。
    /// </summary>
    public int ThreadId { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedTime { get; set; }
}
