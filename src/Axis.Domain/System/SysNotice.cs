using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
/// 通知公告。
/// </summary>
[SugarTable("sys_notice", "通知公告")]
[SysTable]
[SugarIndex("index_{table}_status_publish", nameof(Status), OrderByType.Asc)]
public class SysNotice : BaseEntity
{
    /// <summary>标题。</summary>
    [Required]
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "标题", Length = 128)]
    public string Title { get; set; } = null!;

    /// <summary>内容。</summary>
    [Required]
    [SugarColumn(ColumnDescription = "内容", ColumnDataType = "text")]
    public string Content { get; set; } = null!;

    /// <summary>类型（通知/公告）。</summary>
    [SugarColumn(ColumnDescription = "类型")]
    public NoticeTypeEnum NoticeType { get; set; } = NoticeTypeEnum.Notice;

    /// <summary>发布范围。</summary>
    [SugarColumn(ColumnDescription = "发布范围")]
    public NoticeTargetTypeEnum TargetType { get; set; } = NoticeTargetTypeEnum.All;

    /// <summary>范围目标 Id 列表（JSON 数组，全部时为空）。</summary>
    [MaxLength(2000)]
    [SugarColumn(ColumnDescription = "目标Ids", Length = 2000, IsNullable = true)]
    public string TargetIds { get; set; }

    /// <summary>发布时间（发布后写入）。</summary>
    [SugarColumn(ColumnDescription = "发布时间", IsNullable = true)]
    public DateTime? PublishTime { get; set; }

    /// <summary>排序。</summary>
    [SugarColumn(ColumnDescription = "排序")]
    public int OrderNo { get; set; } = 100;

    /// <summary>状态：启用=已发布，禁用=草稿/下架。</summary>
    [SugarColumn(ColumnDescription = "状态")]
    public StatusEnum Status { get; set; } = StatusEnum.Disable;

    /// <summary>备注。</summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "备注", Length = 256, IsNullable = true)]
    public string Remark { get; set; }
}
