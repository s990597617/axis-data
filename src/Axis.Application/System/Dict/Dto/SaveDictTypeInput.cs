using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.System.Dict.Dto;

/// <summary>
/// 新增或更新字典类型的输入数据。
/// </summary>
/// <remarks>
/// 新增时 <see cref="Id"/> 由服务端忽略；更新时由路由绑定。
/// 格式校验以 Validator 为准；编码唯一性由应用服务校验。
/// </remarks>
public sealed class SaveDictTypeInput
{
    /// <summary>
    /// 字典类型主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 字典类型显示名称；必填，最长 64。
    /// </summary>
    [Required, MaxLength(64)]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 字典类型编码；必填，最长 64，租户内唯一。
    /// </summary>
    [Required, MaxLength(64)]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 是否租户字典；否表示全平台共用。创建后不可改；非超管强制为 true。
    /// </summary>
    public bool IsTenant { get; set; } = true;

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
