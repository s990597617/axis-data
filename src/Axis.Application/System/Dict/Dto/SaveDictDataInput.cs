using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.System.Dict.Dto;

/// <summary>
/// 新增或更新字典数据项的输入数据。
/// </summary>
/// <remarks>
/// 新增时 <see cref="Id"/> 由服务端忽略；更新时由路由绑定。
/// 格式校验以 Validator 为准；同类型下 Value 唯一性由应用服务校验。
/// </remarks>
public sealed class SaveDictDataInput
{
    /// <summary>
    /// 字典数据项主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 所属字典类型主键；必填且须已存在。
    /// </summary>
    public long DictTypeId { get; set; }

    /// <summary>
    /// 展示标签；必填，最长 64。
    /// </summary>
    [Required, MaxLength(64)]
    public string Label { get; set; } = null!;

    /// <summary>
    /// 存储值；必填，最长 64，同一类型下唯一。
    /// </summary>
    [Required, MaxLength(64)]
    public string Value { get; set; } = null!;

    /// <summary>
    /// 前端标签样式名；可为空，最长 32。
    /// </summary>
    [MaxLength(32)]
    public string? TagStyle { get; set; }

    /// <summary>
    /// 列表展示样式类名；可为空，最长 32。
    /// </summary>
    [MaxLength(32)]
    public string? ListClass { get; set; }

    /// <summary>
    /// 是否设为默认项；为 true 时会清除同类型其它默认项。
    /// </summary>
    public bool IsDefault { get; set; }

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
