using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
/// 定时任务定义（业务配置源；调度器按此同步）。
/// </summary>
[SugarTable("sys_job", "定时任务")]
[SysTable]
[SugarIndex("uk_{table}_job_code", nameof(JobCode), OrderByType.Asc, true)]
public class SysJob : BaseEntity
{
    /// <summary>
    /// 任务编码（对应 IAxisJob.Code，全局唯一）。
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "任务编码", Length = 64)]
    public string JobCode { get; set; } = null!;

    /// <summary>
    /// 任务名称。
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "任务名称", Length = 64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 任务分组。
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "任务分组", Length = 64, IsNullable = true)]
    public string GroupName { get; set; }

    /// <summary>
    /// Cron 表达式。
    /// </summary>
    [Required]
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "Cron表达式", Length = 128)]
    public string Cron { get; set; } = null!;

    /// <summary>
    /// 是否允许并发执行。
    /// </summary>
    [SugarColumn(ColumnDescription = "允许并发")]
    public bool Concurrent { get; set; }

    /// <summary>
    /// 任务参数（JSON，可选）。
    /// </summary>
    [MaxLength(2000)]
    [SugarColumn(ColumnDescription = "任务参数", Length = 2000, IsNullable = true)]
    public string Params { get; set; }

    /// <summary>
    /// 是否内置（内置禁止删除）。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否内置")]
    public bool IsBuiltin { get; set; }

    /// <summary>
    /// 排序。
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 状态（启用/停用）。
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public StatusEnum Status { get; set; } = StatusEnum.Disable;

    /// <summary>
    /// 备注。
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "备注", Length = 256, IsNullable = true)]
    public string Remark { get; set; }
}
