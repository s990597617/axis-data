using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
///     租户
/// </summary>
[SugarTable("sys_tenant", "租户")]
[SysTable]
[SugarIndex("index_{table}_code", nameof(Code), OrderByType.Asc)]
public class SysTenant : BaseEntity
{
    /// <summary>
    ///     租户名称
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "租户名称", Length = 64)]
    public string Name { get; set; } = null!;

    /// <summary>
    ///     租户编码
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "租户编码", Length = 64)]
    public string Code { get; set; } = null!;

    /// <summary>
    ///     隔离类型
    /// </summary>
    [SugarColumn(ColumnDescription = "隔离类型")]
    public TenantTypeEnum Type { get; set; } = TenantTypeEnum.Id;

    /// <summary>
    ///     关联数据库连接 ConfigId（Db 模式）
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "数据库配置Id", Length = 64, IsNullable = true)]
    public string DbConfigId { get; set; }

    /// <summary>
    ///     关联管理员用户 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "管理员用户Id", IsNullable = true)]
    public long? AdminUserId { get; set; }


    /// <summary>
    ///     开通状态（独立库建库/InitTables；Id 隔离创建后即为 Ready）。
    /// </summary>
    [SugarColumn(ColumnDescription = "开通状态")]
    public TenantProvisionStatusEnum ProvisionStatus { get; set; } = TenantProvisionStatusEnum.Ready;

    /// <summary>
    ///     开通失败原因；成功时可为空
    /// </summary>
    [MaxLength(512)]
    [SugarColumn(ColumnDescription = "开通失败原因", Length = 512, IsNullable = true)]
    public string? ProvisionMessage { get; set; }

    /// <summary>
    ///     到期时间
    /// </summary>
    [SugarColumn(ColumnDescription = "到期时间", IsNullable = true)]
    public DateTime? ExpireTime { get; set; }

    /// <summary>
    ///     排序
    /// </summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int OrderNo { get; set; } = 100;

    /// <summary>
    ///     状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态")]
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    ///     备注
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "备注", Length = 256, IsNullable = true)]
    public string Remark { get; set; }
}