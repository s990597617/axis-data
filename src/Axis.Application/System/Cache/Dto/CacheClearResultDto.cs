namespace Axis.Application.System.Cache.Dto;

/// <summary>
/// 缓存清理结果。
/// </summary>
public sealed class CacheClearResultDto
{
    /// <summary>
    /// 是否成功。
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 清理前匹配到的键数量（仅按前缀清理时有意义；可能因上限截断）。
    /// </summary>
    public int MatchedCount { get; set; }

    /// <summary>
    /// 说明。
    /// </summary>
    public string Message { get; set; } = null!;
}
