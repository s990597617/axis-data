namespace Axis.Application.System.Cache.Dto;

/// <summary>
/// 缓存运行概览。
/// </summary>
public sealed class CacheOverviewDto
{
    /// <summary>
    /// 缓存类型（memory / redis / hybrid）。
    /// </summary>
    public string CacheType { get; set; } = null!;

    /// <summary>
    /// 全局键前缀。
    /// </summary>
    public string GlobalPrefix { get; set; } = null!;

    /// <summary>
    /// 键分隔符。
    /// </summary>
    public string Separator { get; set; } = null!;

    /// <summary>
    /// 当前环境是否允许清空全部缓存。
    /// </summary>
    public bool AllowFlushAll { get; set; }

    /// <summary>
    /// 单次查询键数量上限。
    /// </summary>
    public int MaxKeysPerQuery { get; set; }

    /// <summary>
    /// 已知缓存分组（含实时键数，供前端点选）。
    /// </summary>
    public List<CachePrefixGroupDto> Groups { get; set; } = [];
}
