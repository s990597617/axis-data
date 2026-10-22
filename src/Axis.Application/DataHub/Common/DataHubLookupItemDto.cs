namespace Axis.Application.DataHub.Common;

/// <summary>
/// DataHub 下拉选项（Id 以字符串返回，避免前端精度丢失）。
/// </summary>
public sealed class DataHubLookupItemDto
{
    /// <summary>选项值（实体 Id）。</summary>
    public string Value { get; set; } = null!;

    /// <summary>展示文案，如「名称（编码）」。</summary>
    public string Label { get; set; } = null!;
}
