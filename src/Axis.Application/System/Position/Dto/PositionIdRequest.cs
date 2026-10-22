namespace Axis.Application.System.Position.Dto;

/// <summary>
/// 仅携带岗位主键的路径参数。
/// </summary>
public sealed class PositionIdRequest
{
    /// <summary>
    /// 岗位主键 Id。
    /// </summary>
    public long Id { get; set; }
}
