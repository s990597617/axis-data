using Axis.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Axis.Application.System.Job.Dto;

/// <summary>
/// 保存定时任务。
/// </summary>
public sealed class SaveJobInput
{
    public long Id { get; set; }

    [Required]
    [MaxLength(64)]
    public string JobCode { get; set; } = null!;

    [Required]
    [MaxLength(64)]
    public string Name { get; set; } = null!;

    [MaxLength(64)]
    public string? GroupName { get; set; }

    [Required]
    [MaxLength(128)]
    public string Cron { get; set; } = null!;

    public bool Concurrent { get; set; }

    [MaxLength(2000)]
    public string? Params { get; set; }

    public int OrderNo { get; set; } = 100;

    public StatusEnum Status { get; set; } = StatusEnum.Disable;

    [MaxLength(256)]
    public string? Remark { get; set; }
}
