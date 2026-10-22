using System.ComponentModel.DataAnnotations;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
/// 编码规则流水桶。
/// </summary>
/// <remarks>
/// 唯一键为租户 + 规则 + 周期键 + 范围键。禁止对业务表做无锁 <c>max+1</c>。
/// 本表不走软删除，避免流水行被过滤器隐藏。
/// </remarks>
[SugarTable("sys_code_serial", "编码规则流水")]
[SysTable]
[SugarIndex("uk_{table}_bucket",
    nameof(TenantId), OrderByType.Asc,
    nameof(RuleId), OrderByType.Asc,
    nameof(PeriodKey), OrderByType.Asc,
    nameof(ScopeKey), OrderByType.Asc,
    true)]
public class SysCodeSerial : BaseEntityId, ITenantId
{
    /// <summary>
    /// 租户 Id；与规则取号时的租户一致。
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }

    /// <summary>
    /// 所属规则主键。
    /// </summary>
    [SugarColumn(ColumnDescription = "规则Id")]
    public long RuleId { get; set; }

    /// <summary>
    /// 周期键：<c>never</c> / <c>yyyyMMdd</c> / <c>yyyyMM</c> / <c>yyyy</c>。
    /// </summary>
    [Required]
    [MaxLength(16)]
    [SugarColumn(ColumnDescription = "周期键", Length = 16)]
    public string PeriodKey { get; set; } = null!;

    /// <summary>
    /// 分桶范围键；第一期固定为下划线。
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "范围键", Length = 64)]
    public string ScopeKey { get; set; } = "_";

    /// <summary>
    /// 当前已发放流水值。
    /// </summary>
    [SugarColumn(ColumnDescription = "当前流水")]
    public long CurrentValue { get; set; }
}
