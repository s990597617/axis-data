namespace Axis.Application.System.Menu.Dto;

/// <summary>
/// 仅携带菜单节点主键的路径参数。
/// </summary>
public sealed class MenuIdRequest
{
    /// <summary>
    /// 菜单节点主键 Id。
    /// </summary>
    public long Id { get; set; }
}
