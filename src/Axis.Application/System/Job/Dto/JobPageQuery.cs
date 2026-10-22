using Axis.Domain.Enums;

namespace Axis.Application.System.Job.Dto;

/// <summary>
/// 定时任务分页查询。
/// </summary>
public sealed class JobPageQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? Keyword { get; set; }
    public string? GroupName { get; set; }
    public StatusEnum? Status { get; set; }
}
