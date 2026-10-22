namespace Axis.Application.Demo.Brand.Dto;

/// <summary>
/// 仅携带品牌主键的路径参数。
/// </summary>
public sealed class BrandIdRequest
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }
}
