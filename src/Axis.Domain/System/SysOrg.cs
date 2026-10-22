using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Domain.Interfaces;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
///     组织机构
/// </summary>
[SugarTable("sys_org", "组织机构")]
[SysTable]
[SugarIndex("index_{table}_pid", nameof(ParentId), OrderByType.Asc)]
[SugarIndex("index_{table}_code", nameof(Code), OrderByType.Asc)]
public class SysOrg : BaseEntity, ITenantId
{
    /// <summary>
    ///     租户 Id
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }

    /// <summary>
    ///     父机构 Id（顶级为 0）
    /// </summary>
    [SugarColumn(ColumnDescription = "父机构Id")]
    public long ParentId { get; set; }

    /// <summary>
    ///     机构名称
    /// </summary>
    [Required]
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "机构名称", Length = 64)]
    public string Name { get; set; } = null!;

    /// <summary>
    ///     机构编码
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "机构编码", Length = 64, IsNullable = true)]
    public string Code { get; set; }

    /// <summary>
    ///     机构类型
    /// </summary>
    [SugarColumn(ColumnDescription = "机构类型")]
    public OrgTypeEnum Type { get; set; } = OrgTypeEnum.Department;

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