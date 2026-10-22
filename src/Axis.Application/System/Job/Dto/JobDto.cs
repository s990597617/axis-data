using Axis.Domain.Enums;

namespace Axis.Application.System.Job.Dto;

/// <summary>
/// 定时任务列表项。
/// </summary>
public sealed class JobDto
{
    public long Id { get; set; }
    public string JobCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? GroupName { get; set; }
    public string Cron { get; set; } = null!;
    public bool Concurrent { get; set; }
    public string? Params { get; set; }
    public bool IsBuiltin { get; set; }
    public int OrderNo { get; set; }
    public StatusEnum Status { get; set; }
    public string? Remark { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime? ModifiedTime { get; set; }
    public DateTime? NextFireTime { get; set; }
    public bool ExecutorRegistered { get; set; }
}
