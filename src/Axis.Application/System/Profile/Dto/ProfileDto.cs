using Axis.Domain.Enums;
using Axis.Infrastructure.Author.Internal;

namespace Axis.Application.System.Profile.Dto;

/// <summary>
/// 当前登录用户的个人中心资料。
/// </summary>
public sealed class ProfileDto
{
    /// <summary>
    /// 用户主键。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 登录账号。
    /// </summary>
    public string Account { get; set; } = null!;

    /// <summary>
    /// 真实姓名。
    /// </summary>
    public string RealName { get; set; } = null!;

    /// <summary>
    /// 昵称；可为空。
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 头像地址；可为空。
    /// </summary>
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
    /// 手机号；可为空。
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 邮箱；可为空。
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 备注；可为空。
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 工号；可为空。
    /// </summary>
    public string? JobNum { get; set; }

    /// <summary>
    /// 入职日期；可为空。
    /// </summary>
    public DateTime? JoinDate { get; set; }

    /// <summary>
    /// 账号类型。
    /// </summary>
    public AccountTypeEnum AccountType { get; set; }

    /// <summary>
    /// 所属机构 Id。
    /// </summary>
    public long OrgId { get; set; }

    /// <summary>
    /// 所属机构名称；可为空。
    /// </summary>
    public string? OrgName { get; set; }

    /// <summary>
    /// 所属岗位 Id。
    /// </summary>
    public long PosId { get; set; }

    /// <summary>
    /// 所属岗位名称；可为空。
    /// </summary>
    public string? PosName { get; set; }

    /// <summary>
    /// 已启用角色名称集合。
    /// </summary>
    public IReadOnlyList<string> RoleNames { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 账号状态。
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// 最近登录 IP；可为空。
    /// </summary>
    public string? LastLoginIp { get; set; }

    /// <summary>
    /// 最近登录时间；未登录过时为空。
    /// </summary>
    public DateTime? LastLoginTime { get; set; }

    /// <summary>
    /// 账号创建时间。
    /// </summary>
    public DateTime CreatedTime { get; set; }
}
