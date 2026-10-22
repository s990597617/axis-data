using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Domain.DataHub;

/// <summary>
/// 采集运行记录。
/// </summary>
[SugarTable("dh_ingest_run", "采集运行")]
[SysTable]
[SugarIndex("index_{table}_code", nameof(Code), OrderByType.Asc)]
public class DhIngestRun : BaseEntity, ITenantId
{
    /// <summary>
    /// 租户 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }

    /// <summary>
    /// 任务 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "任务Id")]
    public long JobId { get; set; }

    /// <summary>
    /// 名称。
    /// </summary>
    [Required]
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "名称", Length = 128)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 编码。
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "编码", Length = 64)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 运行状态。
    /// </summary>
    [SugarColumn(ColumnDescription = "运行状态")]
    public DhRunStatusEnum RunStatus { get; set; } = DhRunStatusEnum.Pending;

    /// <summary>
    /// 开始时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "开始时间", IsNullable = true)]
    public DateTime? StartedTime { get; set; }

    /// <summary>
    /// 结束时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "结束时间", IsNullable = true)]
    public DateTime? FinishedTime { get; set; }

    /// <summary>
    /// 读取行数。
    /// </summary>
    [SugarColumn(ColumnDescription = "读取行数")]
    public long RowCountRead { get; set; }

    /// <summary>
    /// 变更行数。
    /// </summary>
    [SugarColumn(ColumnDescription = "变更行数")]
    public long RowCountChanged { get; set; }

    /// <summary>
    /// 错误信息。
    /// </summary>
    [MaxLength(1024)]
    [SugarColumn(ColumnDescription = "错误信息", Length = 1024, IsNullable = true)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 水位线。
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "水位线", Length = 512, IsNullable = true)]
    public string? Watermark { get; set; }

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
