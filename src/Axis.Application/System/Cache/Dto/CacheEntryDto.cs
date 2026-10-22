namespace Axis.Application.System.Cache.Dto;

/// <summary>
/// 缓存条目详情。
/// </summary>
public sealed class CacheEntryDto
{
    /// <summary>
    /// 完整缓存键。
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// 是否存在。
    /// </summary>
    public bool Exists { get; set; }

    /// <summary>
    /// 剩余过期秒数；负数表示永久或未知。
    /// </summary>
    public double ExpirationSeconds { get; set; }

    /// <summary>
    /// 运行时类型名（若可识别）。
    /// </summary>
    public string ValueType { get; set; }

    /// <summary>
    /// 值预览（JSON / 文本，可能截断）。
    /// </summary>
    public string ValuePreview { get; set; }

    /// <summary>
    /// 预览是否被截断。
    /// </summary>
    public bool PreviewTruncated { get; set; }
}
