namespace Axis.Application.DataHub.Source.Dto;

/// <summary>
/// 列出数据源对象列的请求。
/// </summary>
public sealed class ListSourceColumnsRequest
{
    /// <summary>数据源 Id（路径参数）。</summary>
    public long Id { get; set; }

    /// <summary>对象名（表或视图名，可含 Schema）。</summary>
    public string ObjectName { get; set; } = null!;
}