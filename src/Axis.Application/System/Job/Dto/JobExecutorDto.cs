namespace Axis.Application.System.Job.Dto;

/// <summary>
/// 已注册任务执行器（供创建/编辑任务时选择）。
/// </summary>
public sealed class JobExecutorDto
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? GroupName { get; set; }

    public string? Cron { get; set; }

    public bool Concurrent { get; set; }

    public string? DefaultParams { get; set; }

    public bool Builtin { get; set; }

    public int OrderNo { get; set; }

    public bool Enabled { get; set; }

    public string? Remark { get; set; }

    /// <summary>
    /// 是否标注了 AxisJobAttribute（可自动入库）。
    /// </summary>
    public bool HasMetadata { get; set; }
}
