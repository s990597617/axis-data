using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.Demo.Brand.Dto;

/// <summary>
/// 新增或更新品牌的输入数据。
/// </summary>
public sealed class SaveBrandInput
{
    /// <summary>
    /// 主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 岗位名称.
    /// </summary>
    [Required, MaxLength(64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 岗位编码.
    /// </summary>
    [MaxLength(64)]
    public string Code { get; set; }

    /// <summary>
    /// 排序.
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 状态.
    /// </summary>
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 备注.
    /// </summary>
    [MaxLength(256)]
    public string Remark { get; set; }
}
