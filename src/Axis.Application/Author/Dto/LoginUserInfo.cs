using Axis.Domain.Enums;
using Axis.Infrastructure.Author.Internal;

namespace Axis.Application.Author.Dto;

/// <summary>
/// 当前登录用户的完整资料，含机构、角色、按钮权限与菜单路由树。
/// </summary>
/// <remarks>
/// 由 <see cref="Author.IAuthorProfileAppService.GetUserInfoAsync"/> 装配；不包含令牌本身。
/// </remarks>
public sealed class LoginUserInfo
{
    /// <summary>
    /// 用户主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 登录账号。
    /// </summary>
    public string Account { get; set; } = null!;

    /// <summary>
    /// 真实姓名；可为空。
    /// </summary>
    public string? RealName { get; set; }

    /// <summary>
    /// 手机号；可为空。
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// 账号类型（含超级管理员）。
    /// </summary>
    public AccountTypeEnum AccountType { get; set; } = AccountTypeEnum.User;

    /// <summary>
    /// 所属租户 Id；超级管理员可为空。
    /// </summary>
    public long? TenantId { get; set; }

    /// <summary>
    /// 头像地址；可为空。
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// 个人简介；可为空。
    /// </summary>
    public string? Introduction { get; set; }

    /// <summary>
    /// 地址；可为空。
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 电子签名；可为空。
    /// </summary>
    public string? Signature { get; set; }

    /// <summary>
    /// 所属机构 Id。
    /// </summary>
    public long OrgId { get; set; }

    /// <summary>
    /// 所属机构名称；机构缺失时为空。
    /// </summary>
    public string? OrgName { get; set; }

    /// <summary>
    /// 所属机构类型字符串；机构缺失时为空。
    /// </summary>
    public string? OrgType { get; set; }

    /// <summary>
    /// 岗位名称；可为空。
    /// </summary>
    public string? PosName { get; set; }

    /// <summary>
    /// 已启用角色编码集合。
    /// </summary>
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 按钮级权限集合；超级管理员为 <c>["*"]</c>。
    /// </summary>
    public IReadOnlyList<string> Buttons { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 菜单路由树（已剔除按钮节点）。
    /// </summary>
    public IReadOnlyList<LoginMenuTreeNode> MenuTrees { get; set; } = Array.Empty<LoginMenuTreeNode>();
}
