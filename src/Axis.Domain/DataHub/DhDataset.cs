using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Domain.DataHub;

/// <summary>
/// 数据集。
/// </summary>
[SugarTable("dh_dataset", "数据集")]
[SysTable]
[SugarIndex("index_{table}_code", nameof(Code), OrderByType.Asc)]
public class DhDataset : BaseEntity, ITenantId
{
    /// <summary>
    /// 租户 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }

    /// <summary>
    /// 数据源 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "数据源Id")]
    public long SourceId { get; set; }

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
    /// 对象类型。
    /// </summary>
    [SugarColumn(ColumnDescription = "对象类型")]
    public DhObjectTypeEnum ObjectType { get; set; } = DhObjectTypeEnum.Table;

    /// <summary>
    /// 对象名（表/视图等）。
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "对象名", Length = 256, IsNullable = true)]
    public string? ObjectName { get; set; }

    /// <summary>
    /// 自定义 SQL。
    /// </summary>
    [SugarColumn(ColumnDescription = "自定义SQL", ColumnDataType = "text", IsNullable = true)]
    public string? CustomSql { get; set; }

    /// <summary>
    /// 主键字段（逗号分隔）。
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "主键字段", Length = 256, IsNullable = true)]
    public string? PrimaryKeyFields { get; set; }

    /// <summary>
    /// 增量字段。
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "增量字段", Length = 128, IsNullable = true)]
    public string? IncrementalField { get; set; }

    /// <summary>
    /// 更新策略。
    /// </summary>
    [SugarColumn(ColumnDescription = "更新策略")]
    public DhUpdateStrategyEnum UpdateStrategy { get; set; } = DhUpdateStrategyEnum.Full;

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
