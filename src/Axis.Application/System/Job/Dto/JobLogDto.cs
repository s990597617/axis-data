using Axis.Domain.Enums;

namespace Axis.Application.System.Job.Dto;

/// <summary>
/// 定时任务执行日志。
/// </summary>
public sealed class JobLogDto
{
    public long Id { get; set; }
    public long JobId { get; set; }
    public string JobCode { get; set; } = null!;
    public string? JobName { get; set; }
    public JobExecStatusEnum Status { get; set; }
    public JobTriggerTypeEnum TriggerType { get; set; }
    public DateTime StartedTime { get; set; }
    public DateTime? EndedTime { get; set; }
    public long? DurationMs { get; set; }
    public string? Message { get; set; }
    public string? Exception { get; set; }
}
