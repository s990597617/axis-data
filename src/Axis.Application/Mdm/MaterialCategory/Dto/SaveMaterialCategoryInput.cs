using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.Mdm.MaterialCategory.Dto;

/// <summary>
/// 新增或更新物料分类的输入数据。
/// </summary>
/// <remarks>
/// 新增时 <see cref="Id"/> 由服务端忽略；更新时由路由绑定。
/// 格式校验以 Validator 为准；编码唯一性与树环路由应用服务校验。
/// </remarks>
public sealed class SaveMaterialCategoryInput
{
    /// <summary>
    /// 分类主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 上级分类 Id；根节点传 0。
    /// </summary>
    public long ParentId { get; set; }

    /// <summary>
    /// 分类名称；必填，最长 64。
    /// </summary>
    [Required, MaxLength(64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 分类编码；必填，最长 64，租户内唯一。
    /// </summary>
    [Required, MaxLength(64)]
    public string Code { get; set; } = null!;

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
