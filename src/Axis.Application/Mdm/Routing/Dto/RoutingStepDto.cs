namespace Axis.Application.Mdm.Routing.Dto;

/// <summary>
/// 工艺路线工序行。
/// </summary>
public sealed class RoutingStepDto
{
    /// <summary>
    /// 行主键；列表展示可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 工序序号。
    /// </summary>
    public int Seq { get; set; }

    /// <summary>
    /// 工序 Id。
    /// </summary>
    public long ProcessId { get; set; }

    /// <summary>
    /// 工序编码。
    /// </summary>
    public string ProcessCode { get; set; } = null!;

    /// <summary>
    /// 工序名称。
    /// </summary>
    public string ProcessName { get; set; } = null!;

    /// <summary>
    /// 标准工时，单位分钟。
    /// </summary>
    public int StandardMinutes { get; set; }

    /// <summary>
    /// 是否报工点。
    /// </summary>
    public bool IsReportPoint { get; set; }

    /// <summary>
    /// 备注。
    /// </summary>
    public string? Remark { get; set; }
}
