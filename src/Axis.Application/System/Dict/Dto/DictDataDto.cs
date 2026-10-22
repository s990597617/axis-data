using Axis.Domain.Enums;

namespace Axis.Application.System.Dict.Dto;

/// <summary>
/// 字典数据项列表与编辑回显数据。
/// </summary>
/// <remarks>
/// 仅暴露管理端展示与编辑所需字段，不直接返回领域实体。
/// </remarks>
public sealed class DictDataDto
{
    /// <summary>
    /// 字典数据项主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 所属字典类型主键。
    /// </summary>
    public long DictTypeId { get; set; }

    /// <summary>
    /// 展示标签文案。
    /// </summary>
    public string Label { get; set; } = null!;

    /// <summary>
    /// 存储值；同一字典类型下唯一。
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// 前端标签样式名；可为空。
    /// </summary>
    public string? TagStyle { get; set; }

    /// <summary>
    /// 列表展示样式类名；可为空。
    /// </summary>
    public string? ListClass { get; set; }

    /// <summary>
    /// 是否为该类型下的默认选项。
    /// </summary>
    public bool IsDefault { get; set; }

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
}
