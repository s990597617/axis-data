namespace Axis.Application.DataHub.Dataset.Dto;

/// <summary>
/// 仅携带数据集主键的路径参数。
/// </summary>
public sealed class DatasetIdRequest
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }
}
