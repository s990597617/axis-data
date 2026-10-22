using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Domain.DataHub;

/// <summary>
/// 采集任务。
/// </summary>
[SugarTable("dh_ingest_job", "采集任务")]
[SysTable]
[SugarIndex("index_{table}_code", nameof(Code), OrderByType.Asc)]
public class DhIngestJob : BaseEntity, ITenantId
{
    /// <summary>
    /// 租户 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }

    /// <summary>
    /// 数据集 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "数据集Id")]
    public long DatasetId { get; set; }

    /// <summary>
    /// 名称。
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "名称", Length = 64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 编码。
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "编码", Length = 64)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 策略。
    /// </summary>
    [SugarColumn(ColumnDescription = "策略")]
    public DhUpdateStrategyEnum Strategy { get; set; } = DhUpdateStrategyEnum.Full;

    /// <summary>
    /// Cron 表达式。
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "Cron表达式", Length = 64, IsNullable = true)]
    public string? CronExpression { get; set; }

    /// <summary>
    /// 批大小。
    /// </summary>
    [SugarColumn(ColumnDescription = "批大小")]
    public int BatchSize { get; set; } = 1000;

    /// <summary>
    /// 排序。
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 状态。
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 备注。
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "备注", Length = 256, IsNullable = true)]
    public string? Remark { get; set; }
}
