namespace Axis.Application.System.Cache.Dto;

/// <summary>
/// 按前缀查询缓存键结果。
/// </summary>
public sealed class CacheKeyListDto
{
    /// <summary>
    /// 查询前缀。
    /// </summary>
    public string Prefix { get; set; } = null!;

    /// <summary>
    /// 匹配总数（截断前）。
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 是否因上限被截断。
    /// </summary>
    public bool Truncated { get; set; }

    /// <summary>
    /// 键列表。
    /// </summary>
    public List<CacheKeyItemDto> Items { get; set; } = [];
}
