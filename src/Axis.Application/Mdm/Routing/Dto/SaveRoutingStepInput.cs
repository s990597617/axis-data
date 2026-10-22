using System.ComponentModel.DataAnnotations;

namespace Axis.Application.Mdm.Routing.Dto;

/// <summary>
/// 保存工艺路线时的单行工序。
/// </summary>
public sealed class SaveRoutingStepInput
{
    /// <summary>
    /// 工序序号；同一路线内唯一且大于 0。
    /// </summary>
    public int Seq { get; set; }

    /// <summary>
    /// 工序主键。
    /// </summary>
    public long ProcessId { get; set; }

    /// <summary>
    /// 标准工时，单位分钟；允许 0。
    /// </summary>
    public int StandardMinutes { get; set; }

    /// <summary>
    /// 是否报工点。
    /// </summary>
    public bool IsReportPoint { get; set; } = true;

    /// <summary>
    /// 备注；可为空，最长 256。
    /// </summary>
    [MaxLength(256)]
    public string? Remark { get; set; }
}
