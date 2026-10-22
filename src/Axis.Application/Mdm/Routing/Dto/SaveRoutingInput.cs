using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.Mdm.Routing.Dto;

/// <summary>
/// 新增或更新工艺路线（含工序行）的输入数据。
/// </summary>
public sealed class SaveRoutingInput
{
    /// <summary>
    /// 主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 路线名称；必填，最长 64。
    /// </summary>
    [Required, MaxLength(64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 路线编码；新增留空则按规则 RTG 取号；非空时租户内唯一。
    /// </summary>
    [MaxLength(64)]
    public string? Code { get; set; }

    /// <summary>
    /// 工序行；至少一行，且须含一个报工点。
    /// </summary>
    public List<SaveRoutingStepInput> Steps { get; set; } = [];

    /// <summary>
    /// 排序号；默认 100。
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 启用状态；默认启用。
    /// </summary>
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 备注；可为空，最长 256。
    /// </summary>
    [MaxLength(256)]
    public string? Remark { get; set; }
}
