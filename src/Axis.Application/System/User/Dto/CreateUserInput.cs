using System.ComponentModel.DataAnnotations;
using Axis.Domain.Enums;
using Axis.Infrastructure.Author.Internal;

namespace Axis.Application.System.User.Dto;

/// <summary>
/// 创建用户的输入数据。
/// </summary>
/// <remarks>
/// 格式校验以 Validator 为准；账号唯一性由应用服务校验。
/// 初始密码由服务端使用 <see cref="Domain.System.UserAccountDefaults.InitialPassword"/>，不接受前端传入。
/// </remarks>
public sealed class CreateUserInput
{
    /// <summary>
    /// 所属机构 Id；必填且须大于 0。
    /// </summary>
    [Required(ErrorMessage = "所属机构不能为空")]
    public long OrgId { get; set; }

    /// <summary>
    /// 所属岗位 Id；未分配时可为 0。
    /// </summary>
    public long PosId { get; set; }

    /// <summary>
    /// 直属上级用户 Id；可为空。
    /// </summary>
    public long? ManagerUserId { get; set; }

    /// <summary>
    /// 登录账号；必填，最长 32，租户内唯一。
    /// </summary>
    [Required(ErrorMessage = "账号不能为空")]
    [MaxLength(32)]
    public string Account { get; set; } = null!;

    /// <summary>
    /// 真实姓名；必填，最长 32。
    /// </summary>
    [Required(ErrorMessage = "真实姓名不能为空")]
    [MaxLength(32)]
    public string RealName { get; set; } = null!;

    /// <summary>
    /// 昵称；必填，最长 32。
    /// </summary>
    [Required(ErrorMessage = "昵称不能为空")]
    [MaxLength(32)]
    public string NickName { get; set; } = null!;

    /// <summary>
    /// 头像地址；可为空，最长 512。
    /// </summary>
    [MaxLength(512)]
    public string? Avatar { get; set; }

    /// <summary>
    /// 性别；默认未知。
    /// </summary>
    public GenderEnum Sex { get; set; } = GenderEnum.Unknown;

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
    /// 身份证号；可为空，最长 32。
    /// </summary>
    [MaxLength(32)]
    public string? IdCardNum { get; set; }

    /// <summary>
    /// 工号；可为空，最长 32。
    /// </summary>
    [MaxLength(32)]
    public string? JobNum { get; set; }

    /// <summary>
    /// 入职日期；可为空。
    /// </summary>
    public DateTime? JoinDate { get; set; }

    /// <summary>
    /// 账号类型；默认普通用户。
    /// </summary>
    public AccountTypeEnum AccountType { get; set; } = AccountTypeEnum.User;

    /// <summary>
    /// 排序号；默认 100。
    /// </summary>
    public int OrderNo { get; set; } = 100;

    /// <summary>
    /// 启用状态；默认启用。
    /// </summary>
    public StatusEnum Status { get; set; } = StatusEnum.Enable;

    /// <summary>
    /// 备注；可为空，最长 256。
    /// </summary>
    [MaxLength(256)]
    public string? Remark { get; set; }

    /// <summary>
    /// 分配的角色 Id 列表；可为空表示暂不授权。
    /// </summary>
    public List<long> RoleIds { get; set; } = [];
}
