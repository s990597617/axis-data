namespace Axis.Application.DataHub.IngestRun.Dto;

/// <summary>
/// 仅携带采集运行主键的路径参数。
/// </summary>
public sealed class IngestRunIdRequest
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }
}
