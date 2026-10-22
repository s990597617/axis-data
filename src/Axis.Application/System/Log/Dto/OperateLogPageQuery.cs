namespace Axis.Application.System.Log.Dto;

/// <summary>
/// 操作日志分页查询条件。
/// </summary>
public sealed class OperateLogPageQuery
{
    /// <summary>
    /// 页码，从 1 开始。
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页条数；默认 20。
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 关键字；模糊匹配账号、功能描述、请求地址、IP；可为空。
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// HTTP 方法；为空表示不过滤。
    /// </summary>
    public string? HttpMethod { get; set; }

    /// <summary>
    /// 开始时间（含）；为空表示不限制。
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束时间（含）；为空表示不限制。
    /// </summary>
    public DateTime? EndTime { get; set; }
}
