using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Attributes;
using SqlSugar;

namespace Axis.Domain.System;

/// <summary>
/// 用户收件箱（写扩散：每个收件人一条）。
/// </summary>
/// <remarks>
/// 通知公告、站内信及后续业务消息（报警、工单等）统一落入本表。
/// <list type="bullet">
/// <item><see cref="SourceType"/> + <see cref="SourceId"/> 追溯原始业务单据，便于重发/撤回。</item>
/// <item><see cref="LinkPath"/> + <see cref="BizId"/> 供前端点击跳转；路径由发送方约定，收件箱不解析业务。</item>
/// <item><see cref="ExtraJson"/> 扩展载荷，避免频繁加列。</item>
/// </list>
/// </remarks>
[SugarTable("sys_inbox", "用户收件箱")]
[SysTable]
[SugarIndex("index_{table}_user_read_time", nameof(UserId), OrderByType.Asc, nameof(IsRead), OrderByType.Asc, nameof(CreatedTime), OrderByType.Desc)]
[SugarIndex("index_{table}_user_time", nameof(UserId), OrderByType.Asc, nameof(CreatedTime), OrderByType.Desc)]
[SugarIndex("index_{table}_user_priority_time", nameof(UserId), OrderByType.Asc, nameof(Priority), OrderByType.Desc, nameof(CreatedTime), OrderByType.Desc)]
[SugarIndex("uk_{table}_user_source", nameof(UserId), OrderByType.Asc, nameof(SourceType), OrderByType.Asc, nameof(SourceId), OrderByType.Asc, true)]
[SugarIndex("index_{table}_source", nameof(SourceType), OrderByType.Asc, nameof(SourceId), OrderByType.Asc)]
public class SysInbox : BaseEntityId
{
    /// <summary>
    /// 收件人用户 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "收件人Id")]
    public long UserId { get; set; }

    /// <summary>
    /// 标题快照。
    /// </summary>
    [Required]
    [MaxLength(128)]
    [SugarColumn(ColumnDescription = "标题", Length = 128)]
    public string Title { get; set; } = null!;

    /// <summary>
    /// 列表摘要；可为空。
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "摘要", Length = 256, IsNullable = true)]
    public string? Summary { get; set; }

    /// <summary>
    /// 正文快照（详情展示；业务跳转场景可只填摘要）。
    /// </summary>
    [SugarColumn(ColumnDescription = "内容", ColumnDataType = "text", IsNullable = true)]
    public string? Content { get; set; }

    /// <summary>
    /// 消息来源类型。
    /// </summary>
    [SugarColumn(ColumnDescription = "来源类型")]
    public InboxSourceTypeEnum SourceType { get; set; }

    /// <summary>
    /// 来源业务主键（公告 Id / 站内信 Id / 报警 Id 等）；无来源时为 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "来源Id")]
    public long SourceId { get; set; }

    /// <summary>
    /// 业务模块编码（如 alarm / workOrder）；供前端分发，可为空。
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "业务模块", Length = 64, IsNullable = true)]
    public string BizModule { get; set; }

    /// <summary>
    /// 业务子类型；可为空。
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "业务类型", Length = 64, IsNullable = true)]
    public string BizType { get; set; }

    /// <summary>
    /// 业务单据 Id（跳转用）；可与 SourceId 相同。
    /// </summary>
    [SugarColumn(ColumnDescription = "业务Id", IsNullable = true)]
    public long? BizId { get; set; }

    /// <summary>
    /// 前端路由路径，支持 <c>{bizId}</c> 占位，如 <c>/ops/work-order/{bizId}</c>。
    /// </summary>
    [MaxLength(256)]
    [SugarColumn(ColumnDescription = "跳转路径", Length = 256, IsNullable = true)]
    public string LinkPath { get; set; }

    /// <summary>
    /// 扩展 JSON 载荷；结构由发送方约定。
    /// </summary>
    [SugarColumn(ColumnDescription = "扩展JSON", ColumnDataType = "text", IsNullable = true)]
    public string ExtraJson { get; set; }

    /// <summary>
    /// 发件人 Id；系统消息可为 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "发件人Id")]
    public long SenderId { get; set; }

    /// <summary>
    /// 发件人姓名快照。
    /// </summary>
    [MaxLength(64)]
    [SugarColumn(ColumnDescription = "发件人", Length = 64, IsNullable = true)]
    public string SenderName { get; set; }

    /// <summary>
    /// 是否已读。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否已读")]
    public bool IsRead { get; set; }

    /// <summary>
    /// 阅读时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "阅读时间", IsNullable = true)]
    public DateTime? ReadTime { get; set; }

    /// <summary>
    /// 优先级；数值越大越靠前，默认 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "优先级")]
    public int Priority { get; set; }

    /// <summary>
    /// 投递时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "创建时间")]
    public DateTime CreatedTime { get; set; } = DateTime.Now;
}
