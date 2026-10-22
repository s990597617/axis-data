using Axis.Domain.Enums;

namespace Axis.Application.Mdm.MaterialCategory.Dto;

/// <summary>
/// 物料分类列表项及编辑回显数据。
/// </summary>
public sealed class MaterialCategoryDto
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
    /// 分类编码；租户内唯一。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 排序号；数值越小越靠前。
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 启用状态。
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// 备注；可为空。
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 最后修改时间；从未修改时为空。
    /// </summary>
    public DateTime? ModifiedTime { get; set; }
}
