using Axis.Application.System.Cache.Dto;
using Axis.Infrastructure.Components.Caching.Services;
using Axis.Infrastructure.Constants;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Axis.Application.System.Cache;

/// <summary>
/// 缓存管理：按前缀查看 / 清理，基于 <see cref="ICacheService"/>。
/// </summary>
public sealed class CacheAdminAppService(
    ICacheService cache,
    IOptions<AxisCachingOptions> options,
    IWebHostEnvironment environment)
    : ICacheAdminAppService, IAutoInject
{
    private const int MaxPreviewChars = 8_192;
    private const int HardMaxTake = 500;
    public const string AllPrefixToken = "*";

    private static readonly JsonSerializerOptions PreviewJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <inheritdoc />
    public async Task<CacheOverviewDto> GetOverviewAsync(CancellationToken ct = default)
    {
        var opt = options.Value;
        var groups = await BuildGroupsAsync(ct);
        return new CacheOverviewDto
        {
            CacheType = (opt.CacheType ?? "memory").Trim().ToLowerInvariant(),
            GlobalPrefix = opt.CacheKey.GlobalPrefix,
            Separator = opt.CacheKey.Separator,
            AllowFlushAll = !environment.IsProduction(),
            MaxKeysPerQuery = HardMaxTake,
            Groups = groups
        };
    }

    /// <inheritdoc />
    public async Task<CacheKeyListDto> ListKeysAsync(CacheKeyQuery query, CancellationToken ct = default)
    {
        var prefix = string.IsNullOrWhiteSpace(query.Prefix) ? AllPrefixToken : query.Prefix.Trim();
        var take = Math.Clamp(query.Take <= 0 ? 200 : query.Take, 1, HardMaxTake);
        var keys = await ResolveKeysAsync(prefix, ct);
        var total = keys.Count;
        var sliced = keys.Take(take).ToList();
        var items = new List<CacheKeyItemDto>(sliced.Count);

        foreach (var key in sliced)
        {
            var expiration = await cache.GetExpirationAsync(key, ct);
            items.Add(new CacheKeyItemDto
            {
                Key = key,
                ExpirationSeconds = expiration <= TimeSpan.Zero
                    ? -1
                    : Math.Round(expiration.TotalSeconds, 1)
            });
        }

        return new CacheKeyListDto
        {
            Prefix = prefix,
            Total = total,
            Truncated = total > take,
            Items = items
        };
    }

    /// <inheritdoc />
    public async Task<CacheEntryDto> GetEntryAsync(CacheEntryQuery query, CancellationToken ct = default)
    {
        var key = query.Key.Trim();
        var exists = await cache.ExistsAsync(key, ct);
        if (!exists)
        {
            return new CacheEntryDto
            {
                Key = key,
                Exists = false,
                ExpirationSeconds = -1
            };
        }

        var expiration = await cache.GetExpirationAsync(key, ct);
        object? value = await cache.GetAsync<object>(key, ct);
        var (preview, truncated, typeName) = BuildPreview(value);

        return new CacheEntryDto
        {
            Key = key,
            Exists = true,
            ExpirationSeconds = expiration <= TimeSpan.Zero
                ? -1
                : Math.Round(expiration.TotalSeconds, 1),
            ValueType = typeName,
            ValuePreview = preview,
            PreviewTruncated = truncated
        };
    }

    /// <inheritdoc />
    public async Task RemoveKeyAsync(CacheKeyRequest request, CancellationToken ct = default)
    {
        var key = request.Key.Trim();
        var ok = await cache.RemoveAsync(key, ct);
        if (!ok) throw new BusinessException("删除缓存失败，请稍后重试");
    }

    /// <inheritdoc />
    public async Task<CacheClearResultDto> RemoveByPrefixAsync(CachePrefixRequest request, CancellationToken ct = default)
    {
        var prefix = request.Prefix.Trim();
        if (IsAllToken(prefix))
            throw new BusinessException("请选择具体缓存分组后再清理，或使用「清空全部」");

        var keys = await cache.GetKeysByPrefixAsync(prefix, ct);
        var matched = keys.Count;
        var ok = await cache.RemoveByPrefixAsync(prefix, ct);
        if (!ok) throw new BusinessException("按前缀清理缓存失败，请稍后重试");

        return new CacheClearResultDto
        {
            Success = true,
            MatchedCount = matched,
            Message = matched == 0
                ? $"前缀「{prefix}」下未匹配到缓存键"
                : $"已清理前缀「{prefix}」，清理前匹配约 {matched} 个键"
        };
    }

    /// <inheritdoc />
    public async Task<CacheClearResultDto> FlushAllAsync(CancellationToken ct = default)
    {
        if (environment.IsProduction())
            throw new BusinessException("生产环境禁止清空全部缓存，请改用按前缀清理");

        var ok = await cache.FlushAllAsync(ct);
        if (!ok) throw new BusinessException("清空全部缓存失败");

        return new CacheClearResultDto
        {
            Success = true,
            MatchedCount = -1,
            Message = "已清空全部缓存"
        };
    }

    private async Task<List<CachePrefixGroupDto>> BuildGroupsAsync(CancellationToken ct)
    {
        var defs = CacheCatalog.ResolveAll(options.Value.CacheKey);
        var groups = new List<CachePrefixGroupDto>(defs.Count + 1);
        var allKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var (def, prefix) in defs)
        {
            var keys = await cache.GetKeysByPrefixAsync(prefix, ct);
            foreach (var key in keys) allKeys.Add(key);
            groups.Add(new CachePrefixGroupDto
            {
                Code = def.Code,
                Name = def.Name,
                Prefix = prefix,
                Description = def.Description,
                KeyCount = keys.Count,
                AllowClear = true
            });
        }

        groups.Insert(0, new CachePrefixGroupDto
        {
            Code = "all",
            Name = "全部",
            Prefix = AllPrefixToken,
            Description = "汇总下列已知前缀下的全部缓存键",
            KeyCount = allKeys.Count,
            AllowClear = false
        });

        return groups;
    }

    private async Task<IReadOnlyList<string>> ResolveKeysAsync(string prefix, CancellationToken ct)
    {
        if (!IsAllToken(prefix))
            return await cache.GetKeysByPrefixAsync(prefix, ct);

        var set = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var (_, groupPrefix) in CacheCatalog.ResolveAll(options.Value.CacheKey))
        {
            var keys = await cache.GetKeysByPrefixAsync(groupPrefix, ct);
            foreach (var key in keys) set.Add(key);
        }

        return set.ToList();
    }

    private static bool IsAllToken(string prefix)
        => string.IsNullOrWhiteSpace(prefix)
           || string.Equals(prefix.Trim(), AllPrefixToken, StringComparison.Ordinal);

    private static (string preview, bool truncated, string typeName) BuildPreview(object value)
    {
        if (value is null) return (null, false, null);

        var typeName = value.GetType().FullName;
        string text;
        if (value is string s)
        {
            text = s;
        }
        else
        {
            try
            {
                text = JsonSerializer.Serialize(value, PreviewJsonOptions);
            }
            catch
            {
                text = value.ToString() ?? string.Empty;
            }
        }

        if (text.Length <= MaxPreviewChars)
            return (text, false, typeName);

        return (text[..MaxPreviewChars] + "…", true, typeName);
    }
}
