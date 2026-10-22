using Axis.Application.System.Cache.Dto;

namespace Axis.Application.System.Cache;

/// <summary>
/// 缓存管理应用服务契约。
/// </summary>
public interface ICacheAdminAppService
{
    /// <summary>
    /// 获取缓存运行概览（含已知分组与实时键数）。
    /// </summary>
    Task<CacheOverviewDto> GetOverviewAsync(CancellationToken ct = default);

    /// <summary>
    /// 按前缀列出缓存键。
    /// </summary>
    Task<CacheKeyListDto> ListKeysAsync(CacheKeyQuery query, CancellationToken ct = default);

    /// <summary>
    /// 查看单条缓存详情。
    /// </summary>
    Task<CacheEntryDto> GetEntryAsync(CacheEntryQuery query, CancellationToken ct = default);

    /// <summary>
    /// 删除指定缓存键。
    /// </summary>
    Task RemoveKeyAsync(CacheKeyRequest request, CancellationToken ct = default);

    /// <summary>
    /// 按前缀清理缓存。
    /// </summary>
    Task<CacheClearResultDto> RemoveByPrefixAsync(CachePrefixRequest request, CancellationToken ct = default);

    /// <summary>
    /// 清空全部缓存（非 Production 环境）。
    /// </summary>
    Task<CacheClearResultDto> FlushAllAsync(CancellationToken ct = default);
}
