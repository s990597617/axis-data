using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.Mdm.Material.Dto;

/// <summary>
/// 新增或更新物料的输入数据。
/// </summary>
public sealed class SaveMaterialInput
{
    /// <summary>
    /// 主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 物料名称；必填，最长 128。
    /// </summary>
    [Required, MaxLength(128)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 物料编码；新增留空则按规则 MAT 取号；非空时租户内唯一。
    /// </summary>
    [MaxLength(64)]
    public string? Code { get; set; }

    /// <summary>
    /// 规格型号；可为空，最长 512。
    /// </summary>
    [MaxLength(512)]
    public string? Spec { get; set; }

    /// <summary>
    /// 物料分类 Id；空或 0 表示未分类。
    /// </summary>
    public long? CategoryId { get; set; }

    /// <summary>
    /// 计量单位；必填，须为字典 <c>mdm_material_unit</c> 的 Value。
    /// </summary>
    [Required, MaxLength(32)]
    public string Unit { get; set; } = null!;

    /// <summary>
    /// 来源类型；必填，须为字典 <c>mdm_material_source</c> 的 Value。
    /// </summary>
    [Required, MaxLength(32)]
    public string SourceType { get; set; } = "Buy";

    /// <summary>
    /// 排序号；默认 100。
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 启用状态；默认启用。
    /// </summary>
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 备注；可为空，最长 256。
    /// </summary>
    [MaxLength(256)]
    public string? Remark { get; set; }
}
