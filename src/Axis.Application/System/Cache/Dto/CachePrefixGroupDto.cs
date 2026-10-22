namespace Axis.Application.System.Cache.Dto;

/// <summary>
/// 已知缓存分组（供管理页点选）。
/// </summary>
public sealed class CachePrefixGroupDto
{
    /// <summary>
    /// 分组编码（如 all / dict / config）。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 展示名称。
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 查询/清理使用的前缀；全部为 <c>*</c>。
    /// </summary>
    public string Prefix { get; set; } = null!;

    /// <summary>
    /// 说明。
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 当前匹配键数量。
    /// </summary>
    public int KeyCount { get; set; }

    /// <summary>
    /// 是否允许按该前缀一键清理（全部组不可，请用清空全部）。
    /// </summary>
    public bool AllowClear { get; set; }
}
