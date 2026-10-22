namespace Axis.Application.System.Cache.Dto;

/// <summary>
/// 按键删除缓存。
/// </summary>
public sealed class CacheKeyRequest
{
    /// <summary>
    /// 完整缓存键。
    /// </summary>
    public string Key { get; set; } = null!;
}
