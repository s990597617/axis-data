using Axis.Domain.Enums;

namespace Axis.Application.System.Notice.Dto;

/// <summary>
/// 新增或更新通知公告的输入。
/// </summary>
/// <remarks>
/// 新增时 <see cref="Id"/> 由服务端忽略并重新生成；更新时由路由绑定。
/// 格式校验以 <c>SaveNoticeInputValidator</c> 为准；发布推送由应用服务编排。
/// </remarks>
public sealed class SaveNoticeInput
{
    /// <summary>
    /// 公告主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 公告标题；必填，最长 128。
    /// </summary>
    public string Title { get; set; } = null!;

    /// <summary>
    /// 公告正文；必填，允许富文本。
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// 公告类型；默认 <see cref="NoticeTypeEnum.Notice"/>。
    /// </summary>
    public NoticeTypeEnum NoticeType { get; set; } = NoticeTypeEnum.Notice;

    /// <summary>
    /// 发布范围；默认全部用户。
    /// </summary>
    /// <remarks>
    /// 非 <see cref="NoticeTargetTypeEnum.All"/> 时必须提供 <see cref="TargetIds"/>。
    /// </remarks>
    public NoticeTargetTypeEnum TargetType { get; set; } = NoticeTargetTypeEnum.All;

    /// <summary>
    /// 目标 Id 的 JSON 数组字符串（角色/机构/用户 Id）；全部范围时可为空。
    /// </summary>
    /// <remarks>
    /// 最长 2000；由仓储按 <see cref="TargetType"/> 解析为实际用户集合。
    /// </remarks>
    public string? TargetIds { get; set; }

    /// <summary>
    /// 列表排序号；默认 100，取值 0–9999。
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 启用状态；<see cref="StatusEnum.Enable"/> 表示已发布，默认停用（草稿）。
    /// </summary>
    public StatusEnum Status { get; set; } = StatusEnum.Disable;

    /// <summary>
    /// 备注；可为空，最长 256。
    /// </summary>
    public string? Remark { get; set; }
}
