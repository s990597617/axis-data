namespace Axis.Application.System.Dict.Dto;

/// <summary>
/// 按字典类型编码读取缓存项的查询条件。
/// </summary>
public sealed class DictItemsByCodeQuery
{
    /// <summary>
    /// 字典类型编码；必填，如 <c>gender</c>、<c>common_status</c>。
    /// </summary>
    public string Code { get; set; } = null!;
}
