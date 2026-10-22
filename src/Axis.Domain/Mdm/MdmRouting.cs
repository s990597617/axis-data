using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Domain.Mdm;

/// <summary>
/// 工艺路线头；工单下达时按行展开工序。
/// </summary>
[SugarTable("mdm_routing", "工艺路线")]
[SysTable]
[SugarIndex("uk_{table}_tenant_code", nameof(TenantId), OrderByType.Asc, nameof(Code), OrderByType.Asc, true)]
public class MdmRouting : BaseEntity, ITenantId
{
    /// <summary>
    /// 租户 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }

    /// <summary>
    /// 路线名称。
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "路线名称", Length = 64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 路线编码；租户内唯一。空则按规则 RTG 取号。
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "路线编码", Length = 64)]
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
