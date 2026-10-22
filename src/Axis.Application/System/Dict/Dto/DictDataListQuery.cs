using Axis.Domain.Enums;

namespace Axis.Application.System.Dict.Dto;

/// <summary>
/// 指定字典类型下数据项列表的筛选条件。
/// </summary>
/// <remarks>
/// 不分页；由字典数据列表接口绑定。
/// </remarks>
public sealed class DictDataListQuery
{
    /// <summary>
    /// 所属字典类型主键；必填。
    /// </summary>
    public long DictTypeId { get; set; }

    /// <summary>
    /// 关键字；同时模糊匹配标签与值；可为空表示不按关键字过滤。
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 启用状态；为空表示不按状态过滤。
    /// </summary>
    public StatusEnum? Status { get; set; }
}
