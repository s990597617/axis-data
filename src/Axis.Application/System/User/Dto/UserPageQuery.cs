using Axis.Domain.Enums;

namespace Axis.Application.System.User.Dto;

/// <summary>
/// 用户分页列表的筛选与分页条件。
/// </summary>
public sealed class UserPageQuery
{
    /// <summary>
    /// 页码，从 1 开始。
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页条数；默认 20。
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 按所属机构过滤；为空表示不按机构过滤。
    /// </summary>
    public long? OrgId { get; set; }

    /// <summary>
    /// 关键字；同时模糊匹配账号、真实姓名、昵称、手机号；可为空。
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 启用状态；为空表示不按状态过滤。
    /// </summary>
    public StatusEnum? Status { get; set; }
}
