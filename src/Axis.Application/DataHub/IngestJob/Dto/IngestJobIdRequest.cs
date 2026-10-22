namespace Axis.Application.DataHub.IngestJob.Dto;

/// <summary>
/// 仅携带采集任务主键的路径参数。
/// </summary>
public sealed class IngestJobIdRequest
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }
}
