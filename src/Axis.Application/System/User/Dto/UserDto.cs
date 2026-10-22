using Axis.Domain.Enums;
using Axis.Infrastructure.Author.Internal;

namespace Axis.Application.System.User.Dto;

/// <summary>
/// 用户列表项与详情回显数据。
/// </summary>
/// <remarks>
/// 不包含密码等敏感字段；机构名称由联表投影填充。
/// </remarks>
public sealed class UserDto
{
    /// <summary>
    /// 用户主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 乐观锁版本号；编辑提交时原样回传。
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// 所属机构 Id。
    /// </summary>
    public long OrgId { get; set; }

    /// <summary>
    /// 所属机构名称；机构缺失时可能为空字符串。
    /// </summary>
    public string OrgName { get; set; } = null!;

    /// <summary>
    /// 所属岗位 Id；未分配时为 0。
    /// </summary>
    public long PosId { get; set; }

    /// <summary>
    /// 直属上级用户 Id；可为空。
    /// </summary>
    public long? ManagerUserId { get; set; }

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
    /// 身份证号；可为空。
    /// </summary>
    public string? IdCardNum { get; set; }

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
    /// 排序号。
    /// </summary>
    public int OrderNo { get; set; }

    /// <summary>
    /// 启用状态。
    /// </summary>
    public StatusEnum Status { get; set; }

    /// <summary>
    /// 备注；可为空。
    /// </summary>
    public string? Remark { get; set; }

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 最后修改时间；从未修改时为空。
    /// </summary>
    public DateTime? ModifiedTime { get; set; }

    /// <summary>
    /// 已分配角色 Id 列表；列表页可为空，详情回显时填充。
    /// </summary>
    public List<long> RoleIds { get; set; } = [];

    /// <summary>
    /// 已分配角色名称列表；与 <see cref="RoleIds"/> 顺序一致，详情展示用。
    /// </summary>
    public List<string> RoleNames { get; set; } = [];
}
