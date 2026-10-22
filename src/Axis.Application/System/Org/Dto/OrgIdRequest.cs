namespace Axis.Application.System.Org.Dto;

/// <summary>
/// 仅携带机构主键的路径参数。
/// </summary>
public sealed class OrgIdRequest
{
    /// <summary>
    /// 机构主键 Id。
    /// </summary>
    public long Id { get; set; }
}
