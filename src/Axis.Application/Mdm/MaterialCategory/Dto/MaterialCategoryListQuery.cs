using Axis.Domain.Enums;

namespace Axis.Application.Mdm.MaterialCategory.Dto;

/// <summary>
/// 物料分类管理列表的筛选条件。
/// </summary>
/// <remarks>
/// 不分页；返回平铺列表，由前端组装树。
/// </remarks>
public sealed class MaterialCategoryListQuery
{
    /// <summary>
    /// 关键字；同时模糊匹配名称与编码；可为空。
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 启用状态；为空表示不按状态过滤。
    /// </summary>
    public StatusEnum? Status { get; set; }
}
