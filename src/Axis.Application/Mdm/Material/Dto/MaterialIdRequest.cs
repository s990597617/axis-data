namespace Axis.Application.Mdm.Material.Dto;

/// <summary>
/// 仅携带物料主键的路径参数。
/// </summary>
public sealed class MaterialIdRequest
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }
}
