using Axis.Domain.Enums;

namespace Axis.Application.Mdm.MaterialCategory.Dto;

/// <summary>
/// 启用分类组成的树节点，供下拉选择与物料页筛选。
/// </summary>
public sealed class MaterialCategoryTreeNode
{
    /// <summary>
    /// 分类主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 上级分类 Id；根节点为 0。
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 分类名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 分类编码。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 排序号。
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 启用状态；树接口仅返回启用节点。
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// 下级分类节点；无子节点时为空列表。
    /// </summary>
    public List<MaterialCategoryTreeNode> Children { get; set; } = [];
}
