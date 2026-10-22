using Axis.Domain.Enums;

namespace Axis.Application.DataHub.Source.Dto;

/// <summary>
/// 列出数据源对象（表/视图）的请求。
/// </summary>
public sealed class ListSourceObjectsRequest
{
    /// <summary>数据源 Id（路径参数）。</summary>
    public long Id { get; set; }

    /// <summary>对象类型：表=1，视图=2；查询=3 返回空列表。</summary>
    public DhObjectTypeEnum ObjectType { get; set; } = DhObjectTypeEnum.Table;
}