namespace Axis.Application.Mdm.Routing.Dto;

/// <summary>
/// 仅携带工艺路线主键的路径参数。
/// </summary>
public sealed class RoutingIdRequest
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }
}
