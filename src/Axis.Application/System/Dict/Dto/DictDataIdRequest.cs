namespace Axis.Application.System.Dict.Dto;

/// <summary>
/// 仅携带字典数据项主键的路径参数。
/// </summary>
public sealed class DictDataIdRequest
{
    /// <summary>
    /// 字典数据项主键 Id。
    /// </summary>
    public long Id { get; set; }
}
