using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.Demo.Product.Dto;

/// <summary>
/// 新增或更新产品的输入数据。
/// </summary>
public sealed class SaveProductInput
{
    /// <summary>
    /// 主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 产品名称；必填，最长 64。
    /// </summary>
    [Required, MaxLength(64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 产品编码；可为空，最长 64，非空时租户内唯一。
    /// </summary>
    [MaxLength(64)]
    public string? Code { get; set; }

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
