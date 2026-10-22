namespace Axis.Application.System.Online.Dto;

/// <summary>
/// 在线会话分页查询。
/// </summary>
public sealed class OnlineSessionPageQuery
{
    /// <summary>页码。</summary>
    public int Page { get; set; } = 1;

    /// <summary>每页条数。</summary>
    public int PageSize { get; set; } = 20;

    /// <summary>关键字：账号 / 姓名 / IP。</summary>
    public string? Keyword { get; set; }
}
