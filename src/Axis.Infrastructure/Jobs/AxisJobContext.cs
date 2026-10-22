namespace Axis.Infrastructure.Jobs;

/// <summary>
/// 定时任务执行上下文。
/// </summary>
public sealed class AxisJobContext
{
    /// <summary>
    /// 任务主键。
    /// </summary>
    public long JobId { get; init; }

    /// <summary>
    /// 任务编码。
    /// </summary>
    public required string JobCode { get; init; }

    /// <summary>
    /// 任务名称。
    /// </summary>
    public string JobName { get; init; }

    /// <summary>
    /// 任务参数（JSON）。
    /// </summary>
    public string Params { get; init; }

    /// <summary>
    /// 是否手动触发。
    /// </summary>
    public bool IsManual { get; init; }
}
