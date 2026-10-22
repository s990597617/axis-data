using System.ComponentModel.DataAnnotations;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Domain.Mdm;

/// <summary>
/// 工艺路线工序行。
/// </summary>
[SugarTable("mdm_routing_step", "工艺路线工序行")]
[SysTable]
[SugarIndex("uk_{table}_routing_seq", nameof(RoutingId), OrderByType.Asc, nameof(Seq), OrderByType.Asc, true)]
[SugarIndex("index_{table}_process", nameof(ProcessId), OrderByType.Asc)]
public class MdmRoutingStep : BaseEntity, ITenantId
{
    /// <summary>
    /// 租户 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }

    /// <summary>
    /// 工艺路线头 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "路线Id")]
    public long RoutingId { get; set; }

    /// <summary>
    /// 工序 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "工序Id")]
    public long ProcessId { get; set; }

    /// <summary>
    /// 工序序号；同一路线内唯一，建议 10/20/30。
    /// </summary>
    [SugarColumn(ColumnDescription = "序号")]
    public int Seq { get; set; }

    /// <summary>
    /// 标准工时，单位分钟。
    /// </summary>
    [SugarColumn(ColumnDescription = "标准工时分钟")]
    public int StandardMinutes { get; set; }

    /// <summary>
    /// 是否报工点；下达后仅报工点可报工。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否报工点")]
    public bool IsReportPoint { get; set; } = true;

    /// <summary>
    /// 备注。
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "备注", Length = 256, IsNullable = true)]
    public string? Remark { get; set; }
}
