namespace Axis.Application.System.Cache.Dto;

/// <summary>
/// 缓存键列表项。
/// </summary>
public sealed class CacheKeyItemDto
{
    /// <summary>
    /// 完整缓存键。
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// 剩余过期秒数；负数表示永久或未知。
    /// </summary>
    public double ExpirationSeconds { get; set; }
}
