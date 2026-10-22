using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Domain.Mdm;

/// <summary>
/// 物料档案主数据。
/// </summary>
[SugarTable("mdm_material", "物料档案")]
[SysTable]
[SugarIndex("uk_{table}_tenant_code", nameof(TenantId), OrderByType.Asc, nameof(Code), OrderByType.Asc, true)]
[SugarIndex("index_{table}_category", nameof(CategoryId), OrderByType.Asc)]
public class MdmMaterial : BaseEntity, ITenantId
{
    /// <summary>
    /// 租户 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }

    /// <summary>
    /// 物料名称。
    /// </summary>
    [Required]
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "物料名称", Length = 128)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 物料编码；租户内唯一。空则按规则 MAT 取号。
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "物料编码", Length = 64)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 规格型号。
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "规格型号", Length = 512, IsNullable = true)]
    public string? Spec { get; set; }

    /// <summary>
    /// 物料分类 Id；可空，未分类时为 null。
    /// </summary>
    [SugarColumn(ColumnDescription = "分类Id", IsNullable = true)]
    public long? CategoryId { get; set; }

    /// <summary>
    /// 计量单位；存字典 <c>mdm_material_unit</c> 的 Value。
    /// </summary>
    [Required]
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "计量单位", Length = 32)]
    public string Unit { get; set; } = null!;

    /// <summary>
    /// 来源类型；存字典 <c>mdm_material_source</c> 的 Value（Buy / Make / Outsource）。
    /// </summary>
    [Required]
    [MaxLength(32)]
    [SugarColumn(ColumnDescription = "来源类型", Length = 32, DefaultValue = "Buy")]
    public string SourceType { get; set; } = "Buy";

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
