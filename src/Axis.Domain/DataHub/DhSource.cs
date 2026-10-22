using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Domain.DataHub;

/// <summary>
/// 数据源。
/// </summary>
[SugarTable("dh_source", "数据源")]
[SysTable]
[SugarIndex("index_{table}_code", nameof(Code), OrderByType.Asc)]
public class DhSource : BaseEntity, ITenantId
{
    /// <summary>
    /// 租户 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }

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
    /// 数据源类型。
    /// </summary>
    [SugarColumn(ColumnDescription = "数据源类型")]
    public DhSourceTypeEnum SourceType { get; set; } = DhSourceTypeEnum.SqlServer;

    /// <summary>
    /// 密钥引用（config: / env:）。
    /// </summary>
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "密钥引用", Length = 128, IsNullable = true)]
    public string? SecretRef { get; set; }

    /// <summary>
    /// 内联连接串（不走 SecretRef 时使用）。
    /// </summary>
    [MaxLength(1024)]
    [SugarColumn(ColumnDescription = "连接串", Length = 1024, IsNullable = true)]
    public string? ConnectionString { get; set; }

    /// <summary>
    /// 最近一次连通性测试状态。
    /// </summary>
    [SugarColumn(ColumnDescription = "最近测试状态")]
    public DhConnectionTestStatusEnum LastTestStatus { get; set; } = DhConnectionTestStatusEnum.Unknown;

    /// <summary>
    /// 最近一次连通性测试时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "最近测试时间", IsNullable = true)]
    public DateTime? LastTestTime { get; set; }

    /// <summary>
    /// 最近一次连通性测试消息。
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "最近测试消息", Length = 512, IsNullable = true)]
    public string? LastTestMessage { get; set; }

    /// <summary>
    /// 是否只读。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否只读")]
    public bool IsReadOnly { get; set; } = true;

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