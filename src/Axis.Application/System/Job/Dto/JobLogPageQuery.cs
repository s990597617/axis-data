using Axis.Domain.Enums;

namespace Axis.Application.System.Job.Dto;

/// <summary>
/// 任务日志分页查询。
/// </summary>
public sealed class JobLogPageQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public long? JobId { get; set; }
    public string? JobCode { get; set; }
    public JobExecStatusEnum? Status { get; set; }
}
