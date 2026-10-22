namespace Axis.Application.System.Cache.Dto;

/// <summary>
/// 单条缓存详情查询。
/// </summary>
public sealed class CacheEntryQuery
{
    /// <summary>
    /// 完整缓存键。
    /// </summary>
    public string Key { get; set; } = null!;
}
