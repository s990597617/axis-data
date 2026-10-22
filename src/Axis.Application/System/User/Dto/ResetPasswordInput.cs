using System.ComponentModel.DataAnnotations;

namespace Axis.Application.System.User.Dto;

/// <summary>
/// 重置用户密码的输入数据。
/// </summary>
/// <remarks>
/// <see cref="Id"/> 由路由绑定；格式校验以 Validator 为准。
/// </remarks>
public sealed class ResetPasswordInput
{
    /// <summary>
    /// 用户主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 新密码明文；服务端以 BCrypt 哈希后落库。
    /// </summary>
    [Required(ErrorMessage = "新密码不能为空")]
    public string NewPassword { get; set; } = null!;
}
