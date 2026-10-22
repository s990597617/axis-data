using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Domain.Demo;

/// <summary>
/// 产品。
/// </summary>
[SugarTable("demo_product", "产品")]
[SysTable]
[SugarIndex("index_{table}_code", nameof(Code), OrderByType.Asc)]
public class SysProduct : BaseEntity, ITenantId
{
    /// <summary>
    /// 租户 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }

    /// <summary>
    /// 产品名称。
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "产品名称", Length = 64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 产品编码。
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "产品编码", Length = 64, IsNullable = true)]
    public string? Code { get; set; }

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
