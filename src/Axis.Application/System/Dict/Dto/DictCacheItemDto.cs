namespace Axis.Application.System.Dict.Dto;

/// <summary>
/// 业务侧只读字典项（来自缓存）。
/// </summary>
public sealed class DictCacheItemDto
{
    /// <summary>
    /// 展示标签。
    /// </summary>
    public string Label { get; set; } = null!;

    /// <summary>
    /// 存储值。
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// 前端标签样式；可为空。
    /// </summary>
    public string? TagStyle { get; set; }

    /// <summary>
    /// 列表样式类名；可为空。
    /// </summary>
    public string? ListClass { get; set; }

    /// <summary>
    /// 是否默认项。
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 排序号。
    /// </summary>
    public int OrderNo { get; set; }
}
