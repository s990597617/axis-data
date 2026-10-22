namespace Axis.Application.System.Dict.Dto;

/// <summary>
/// 仅携带字典类型主键的路径参数。
/// </summary>
public sealed class DictTypeIdRequest
{
    /// <summary>
    /// 字典类型主键 Id。
    /// </summary>
    public long Id { get; set; }
}
