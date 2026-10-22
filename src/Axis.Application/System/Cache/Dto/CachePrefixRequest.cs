namespace Axis.Application.System.Cache.Dto;

/// <summary>
/// 按前缀清理缓存。
/// </summary>
public sealed class CachePrefixRequest
{
    /// <summary>
    /// 键前缀。
    /// </summary>
    public string Prefix { get; set; } = null!;
}
