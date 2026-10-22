namespace Axis.Application.DataHub.Source.Dto;

/// <summary>
/// 仅携带数据源主键的路径参数。
/// </summary>
public sealed class SourceIdRequest
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }
}
