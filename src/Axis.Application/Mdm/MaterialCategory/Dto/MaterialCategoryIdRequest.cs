namespace Axis.Application.Mdm.MaterialCategory.Dto;

/// <summary>
/// 仅携带物料分类主键的路径参数。
/// </summary>
public sealed class MaterialCategoryIdRequest
{
    /// <summary>
    /// 分类主键 Id。
    /// </summary>
    public long Id { get; set; }
}
