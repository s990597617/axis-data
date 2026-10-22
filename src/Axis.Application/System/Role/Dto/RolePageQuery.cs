using Axis.Domain.Enums;

namespace Axis.Application.System.Role.Dto;

/// <summary>
/// 角色分页列表的筛选与分页条件。
/// </summary>
/// <remarks>
/// 由角色分页接口绑定；关键字同时匹配名称与编码。
/// </remarks>
public sealed class RolePageQuery
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
    /// 关键字；同时模糊匹配角色名称与编码；可为空表示不按关键字过滤。
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 数据权限范围；为空表示不按数据范围过滤。
    /// </summary>
    public DataScopeEnum? DataScope { get; set; }

    /// <summary>
    /// 启用状态；为空表示不按状态过滤。
    /// </summary>
    public StatusEnum? Status { get; set; }
}
