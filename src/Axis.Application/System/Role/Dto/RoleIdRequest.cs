namespace Axis.Application.System.Role.Dto;

/// <summary>
/// 仅携带角色主键的路径或请求体参数。
/// </summary>
/// <remarks>
/// 用于删除、查询授权等只需 Id 的接口。
/// </remarks>
public sealed class RoleIdRequest
{
    /// <summary>
    /// 角色主键 Id。
    /// </summary>
    public long Id { get; set; }
}
