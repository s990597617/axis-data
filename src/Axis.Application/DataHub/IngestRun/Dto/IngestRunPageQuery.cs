using Axis.Domain.Enums;

namespace Axis.Application.DataHub.IngestRun.Dto;

/// <summary>
/// 采集运行分页列表的筛选与分页条件。
/// </summary>
public sealed class IngestRunPageQuery
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
    /// 关键字；模糊匹配字符串业务字段；可为空。
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 启用状态；为空表示不按状态过滤。
    /// </summary>
    public StatusEnum? Status { get; set; }

    /// <summary>
    /// 采集任务 Id；为空表示不按任务过滤。
    /// </summary>
    public long? JobId { get; set; }

}
