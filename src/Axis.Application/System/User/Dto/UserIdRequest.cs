namespace Axis.Application.System.User.Dto;

/// <summary>
/// 仅携带用户主键的路径参数。
/// </summary>
public sealed class UserIdRequest
{
    /// <summary>
    /// 用户主键 Id。
    /// </summary>
    public long Id { get; set; }
}
