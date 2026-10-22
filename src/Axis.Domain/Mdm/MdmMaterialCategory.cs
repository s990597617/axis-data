using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Domain.Mdm;

/// <summary>
/// 物料分类；租户内树形维护。
/// </summary>
[SugarTable("mdm_material_category", "物料分类")]
[SysTable]
[SugarIndex("uk_{table}_tenant_code", nameof(TenantId), OrderByType.Asc, nameof(Code), OrderByType.Asc, true)]
[SugarIndex("index_{table}_pid", nameof(ParentId), OrderByType.Asc)]
public class MdmMaterialCategory : BaseEntity, ITenantId
{
    /// <summary>
    /// 租户 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }

    /// <summary>
    /// 上级分类 Id；根节点为 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "上级分类Id")]
    public long ParentId { get; set; }

    /// <summary>
    /// 分类名称。
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "分类名称", Length = 64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 分类编码；租户内唯一。
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "分类编码", Length = 64)]
    public string Code { get; set; } = null!;

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
