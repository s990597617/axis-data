namespace Axis.Application.Mdm.Routing.Dto;

/// <summary>
/// 工艺路线下一编码预览；不占用流水。
/// </summary>
public sealed class RoutingNextCodeDto
{
    /// <summary>
    /// 预览编码。
    /// </summary>
    public string Code { get; set; } = null!;
}
