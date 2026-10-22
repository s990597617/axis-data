using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;

namespace Axis.Application.System.Profile.Dto;

/// <summary>
/// 更新当前登录用户基础资料的输入。
/// </summary>
/// <remarks>
/// 不修改账号、密码、机构、岗位与角色；组织信息仅可查看。
/// </remarks>
public sealed class UpdateProfileInput
{
    /// <summary>
    /// 真实姓名；必填，最长 32。
    /// </summary>
    [Required(ErrorMessage = "真实姓名不能为空")]
    [MaxLength(32)]
    public string RealName { get; set; } = null!;

    /// <summary>
    /// 昵称；可为空，最长 32。
    /// </summary>
    [MaxLength(32)]
    public string? NickName { get; set; }

    /// <summary>
    /// 头像地址；可为空，最长 512。
    /// </summary>
    [MaxLength(512)]
    public string? Avatar { get; set; }

    /// <summary>
    /// 性别。
    /// </summary>
    public GenderEnum Sex { get; set; }

    /// <summary>
    /// 出生日期；可为空。
    /// </summary>
    public DateTime? Birthday { get; set; }

    /// <summary>
    /// 手机号；可为空，最长 16。
    /// </summary>
    [MaxLength(16)]
    public string? Phone { get; set; }

    /// <summary>
    /// 邮箱；可为空，最长 64。
    /// </summary>
    [MaxLength(64)]
    public string? Email { get; set; }

    /// <summary>
    /// 备注；可为空，最长 256。
    /// </summary>
    [MaxLength(256)]
    public string? Remark { get; set; }
}
