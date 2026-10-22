namespace Axis.Application.Mdm.Material.Dto;

/// <summary>
/// 物料下一编码预览；不占用流水。
/// </summary>
public sealed class MaterialNextCodeDto
{
    /// <summary>
    /// 预览编码。
    /// </summary>
    public string Code { get; set; } = null!;
}
