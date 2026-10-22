using System.ComponentModel.DataAnnotations;

namespace Axis.Application.Author.Dto;

/// <summary>
/// 登录请求（Web 端）
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// 账号
    /// </summary>
    /// <example>admin</example>
    [Required(ErrorMessage = "账号不能为空")]
    [MinLength(2, ErrorMessage = "账号不能少于两个字符")]
    [MaxLength(32)]
    public string Account { get; set; } = null!;

    /// <summary>
    /// 密码（前端 SM2 加密后 HEX 字符串）
    /// </summary>
    /// <example>123456</example>
    [Required(ErrorMessage = "密码不能为空")]
    [MaxLength(1024)]
    public string Password { get; set; } = null!;

    /// <summary>
    /// 验证码 Id（<see cref="CaptchaResponse.Id"/> 回传）
    /// </summary>
    [Required(ErrorMessage = "验证码Id不能为空")]
    public string CodeId { get; set; } = null!;

    /// <summary>
    /// 验证码
    /// </summary>
    [Required(ErrorMessage = "验证码不能为空")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 是否在浏览器关闭后继续保留 Refresh Token Cookie。
    /// </summary>
    public bool RememberMe { get; set; }
}
