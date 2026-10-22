using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
/// 定时任务执行日志。
/// </summary>
[SugarTable("sys_job_log", "定时任务日志")]
[SysTable]
[SugarIndex("index_{table}_job_id", nameof(JobId), OrderByType.Asc)]
[SugarIndex("index_{table}_started", nameof(StartedTime), OrderByType.Desc)]
public class SysJobLog : BaseEntityId
{
    /// <summary>
    /// 任务主键。
    /// </summary>
    [SugarColumn(ColumnDescription = "任务Id")]
    public long JobId { get; set; }

    /// <summary>
    /// 任务编码。
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "任务编码", Length = 64)]
    public string JobCode { get; set; } = null!;

    /// <summary>
    /// 任务名称快照。
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "任务名称", Length = 64, IsNullable = true)]
    public string JobName { get; set; }

    /// <summary>
    /// 执行状态。
    /// </summary>
    [SugarColumn(ColumnDescription = "执行状态")]
    public JobExecStatusEnum Status { get; set; }

    /// <summary>
    /// 触发方式。
    /// </summary>
    [SugarColumn(ColumnDescription = "触发方式")]
    public JobTriggerTypeEnum TriggerType { get; set; }

    /// <summary>
    /// 开始时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "开始时间")]
    public DateTime StartedTime { get; set; }

    /// <summary>
    /// 结束时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "结束时间", IsNullable = true)]
    public DateTime? EndedTime { get; set; }

    /// <summary>
    /// 耗时（毫秒）。
    /// </summary>
    [SugarColumn(ColumnDescription = "耗时毫秒", IsNullable = true)]
    public long? DurationMs { get; set; }

    /// <summary>
    /// 结果摘要。
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "结果摘要", Length = 512, IsNullable = true)]
    public string Message { get; set; }

    /// <summary>
    /// 异常信息（截断）。
    /// </summary>
    [MaxLength(4000)]
    [SugarColumn(ColumnDescription = "异常信息", Length = 4000, IsNullable = true)]
    public string Exception { get; set; }
}
