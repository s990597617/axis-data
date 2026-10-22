namespace Axis.Application.Demo.Product.Dto;

/// <summary>
/// 仅携带产品主键的路径参数。
/// </summary>
public sealed class ProductIdRequest
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }
}
