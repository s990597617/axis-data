namespace Axis.Application.Demo.Sku.Dto;

/// <summary>
/// 仅携带SKU主键的路径参数。
/// </summary>
public sealed class SkuIdRequest
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }
}
