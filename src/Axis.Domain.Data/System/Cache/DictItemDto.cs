namespace Axis.Domain.Data.System.Cache;

/// <summary>
/// 缓存中的字典数据项精简结构，供业务侧下拉与翻译使用。
/// </summary>
/// <remarks>
/// 仅缓存启用状态的数据项；不含审计与租户字段，避免缓存膨胀。
/// </remarks>
public sealed class DictItemDto
{
    /// <summary>
    /// 展示标签文案。
    /// </summary>
    public string Label { get; set; } = null!;

    /// <summary>
    /// 存储值；同一字典类型下唯一。
    /// </summary>
    public string Value { get; set; } = null!;

    /// <summary>
    /// 前端标签样式名；可为空。
    /// </summary>
    public string? TagStyle { get; set; }

    /// <summary>
    /// 列表展示样式类名；可为空。
    /// </summary>
    public string? ListClass { get; set; }

    /// <summary>
    /// 是否为该类型下的默认选项。
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 排序号；数值越小越靠前。
    /// </summary>
    public int OrderNo { get; set; }
}
