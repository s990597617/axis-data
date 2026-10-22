using FastEndpoints;

namespace Axis.Infrastructure.Commands;

/// <summary>
/// 程序访问命令
/// </summary>
public class ApplicationVisCommand : ICommand
{
    /// <summary>
    /// ip 地址
    /// </summary>
    public string RemoteIp { get; set; }

    /// <summary>
    /// ip 端口
    /// </summary>
    public int RemotePort { get; set; }

    /// <summary>
    /// User-Agent
    /// </summary>
    public string UserAgent { get; set; }

    /// <summary>
    /// 端点名称
    /// </summary>
    public string DisplayTitle { get; set; }

    /// <summary>
    /// 请求方法全名
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// 请求方法名称
    /// </summary>
    public string ActionName { get; set; }

    /// <summary>
    /// 请求方式
    /// </summary>
    public string HttpMethod { get; set; }

    /// <summary>
    /// 请求地址
    /// </summary>
    public string RequestUrl { get; set; }

    /// <summary>
    /// 请求参数
    /// </summary>
    public string RequestParam { get; set; }

    /// <summary>
    /// 操作用时
    /// </summary>
    public long Elapsed { get; set; }

    /// <summary>
    /// 线程 id
    /// </summary>
    public int ThreadId { get; set; }

    /// <summary>
    /// 请求跟踪 id
    /// </summary>
    public string TraceId { get; set; }

    /// <summary>
    /// 访问状态
    /// </summary>
    public string State { get; set; }
}