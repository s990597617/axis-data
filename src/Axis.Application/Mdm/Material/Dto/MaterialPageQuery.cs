using Axis.Domain.Enums;

namespace Axis.Application.Mdm.Material.Dto;

/// <summary>
/// 物料分页列表的筛选与分页条件。
/// </summary>
public sealed class MaterialPageQuery
{
    /// <summary>
    /// 页码，从 1 开始。
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页条数；默认 20。
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 关键字；同时模糊匹配名称与编码；可为空。
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 计量单位；为空表示不按单位过滤。
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// 来源类型；为空表示不按来源过滤。
    /// </summary>
    public string? SourceType { get; set; }

    /// <summary>
    /// 物料分类 Id；空或 0 表示不按分类过滤。非空时包含该节点及子孙。
    /// </summary>
    public long? CategoryId { get; set; }

    /// <summary>
    /// 启用状态；为空表示不按状态过滤。
    /// </summary>
    public StatusEnum? Status { get; set; }
}
