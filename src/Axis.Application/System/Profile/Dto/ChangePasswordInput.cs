using System.ComponentModel.DataAnnotations;

namespace Axis.Application.System.Profile.Dto;

/// <summary>
/// 当前登录用户修改密码的输入。
/// </summary>
/// <remarks>
/// 三个密码字段均为前端 SM2 加密后的密文；服务端解密后再做一致性校验与 BCrypt 落库。
/// </remarks>
public sealed class ChangePasswordInput
{
    /// <summary>
    /// 当前密码 SM2 密文。
    /// </summary>
    [Required(ErrorMessage = "当前密码不能为空")]
    [MaxLength(1024)]
    public string OldPassword { get; set; } = null!;

    /// <summary>
    /// 新密码 SM2 密文；服务端解密后以 BCrypt 哈希落库。
    /// </summary>
    [Required(ErrorMessage = "新密码不能为空")]
    [MaxLength(1024)]
    public string NewPassword { get; set; } = null!;

    /// <summary>
    /// 确认新密码 SM2 密文；服务端解密后须与新密码明文一致。
    /// </summary>
    [Required(ErrorMessage = "确认密码不能为空")]
    [MaxLength(1024)]
    public string ConfirmPassword { get; set; } = null!;
}
