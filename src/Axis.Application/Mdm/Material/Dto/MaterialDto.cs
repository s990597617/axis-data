using Axis.Domain.Enums;

namespace Axis.Application.Mdm.Material.Dto;

/// <summary>
/// 物料列表项及编辑回显数据。
/// </summary>
public sealed class MaterialDto
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 物料名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 物料编码；租户内唯一。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 规格型号。
    /// </summary>
    public string? Spec { get; set; }

    /// <summary>
    /// 物料分类 Id；未分类时为空。
    /// </summary>
    public long? CategoryId { get; set; }

    /// <summary>
    /// 物料分类名称；未分类时为空。
    /// </summary>
    public string? CategoryName { get; set; }

    /// <summary>
    /// 计量单位；字典 <c>mdm_material_unit</c> 的 Value。
    /// </summary>
    public string Unit { get; set; } = null!;

    /// <summary>
    /// 来源类型；字典 <c>mdm_material_source</c> 的 Value。
    /// </summary>
    public string SourceType { get; set; } = null!;

    /// <summary>
    /// 排序号。
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 启用状态。
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// 备注。
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
