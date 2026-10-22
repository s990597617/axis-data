using Axis.Application.System.Menu.Dto;
using Axis.Application.System.Role.Dto;
using Axis.Infrastructure.Components.Database.Internal;

namespace Axis.Application.System.Role;

/// <summary>
/// 角色管理应用服务契约。
/// </summary>
/// <remarks>
/// 负责角色主表维护与菜单授权；不直接处理 HTTP / Cookie。
/// </remarks>
public interface IRoleAppService
{
    /// <summary>
    /// 按条件分页查询角色列表。
    /// </summary>
    /// <param name="query">分页与筛选条件；关键字同时匹配名称与编码。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>角色 DTO 分页结果。</returns>
    Task<SqlSugarPagedList<RoleDto>> PageAsync(RolePageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 查询角色详情，并附带已授权菜单树。
    /// </summary>
    /// <param name="id">角色主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>详情聚合 DTO；角色不存在时为 null。</returns>
    /// <remarks>
    /// 超级管理员固定拥有全部权限，此时 <see cref="RoleDetailDto.OwnAllPermissions"/> 为 true，菜单树为空。
    /// </remarks>
    Task<RoleDetailDto?> GetDetailAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 校验编码唯一后创建角色主表记录。
    /// </summary>
    /// <param name="input">新增角色字段；编码须在当前租户内唯一。</param>
    /// <param name="ct">取消令牌。</param>
    /// <remarks>不处理菜单授权；授权见 <see cref="GrantMenusAsync"/>。</remarks>
    /// <exception cref="Infrastructure.Exceptions.BusinessException">编码已存在时抛出。</exception>
    Task CreateAsync(SaveRoleInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新指定角色的主表字段。
    /// </summary>
    /// <param name="id">要更新的角色主键。</param>
    /// <param name="input">更新字段；编码须在当前租户内唯一（排除自身）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="Infrastructure.Exceptions.BusinessException">角色不存在或编码冲突时抛出。</exception>
    Task UpdateAsync(long id, SaveRoleInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除角色及其菜单、机构关联；已分配用户或超级管理员角色不允许删除。
    /// </summary>
    /// <param name="id">要删除的角色主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="Infrastructure.Exceptions.BusinessException">
    /// 角色不存在、为超级管理员或仍绑定用户时抛出。
    /// </exception>
    Task DeleteAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 查询角色已分配的菜单与按钮 Id 列表。
    /// </summary>
    /// <param name="id">角色主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>菜单与按钮 Id 只读列表；无授权时为空列表。</returns>
    /// <exception cref="Infrastructure.Exceptions.BusinessException">角色不存在时抛出。</exception>
    Task<IReadOnlyList<long>> GetMenuIdsAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 查询当前租户可授予角色的菜单平铺列表（租户启用集；超管无租户时返回全部非平台菜单）。
    /// </summary>
    Task<IReadOnlyList<MenuDto>> ListGrantableMenusAsync(CancellationToken ct = default);

    /// <summary>
    /// 覆盖保存角色的菜单与按钮授权。
    /// </summary>
    /// <param name="id">角色主键。</param>
    /// <param name="menuIds">目标菜单与按钮 Id；空集合表示清空授权。</param>
    /// <param name="ct">取消令牌。</param>
    /// <remarks>超级管理员角色不允许单独授权；菜单须属于角色所属租户的启用集。</remarks>
    /// <exception cref="Infrastructure.Exceptions.BusinessException">
    /// 角色不存在、为超级管理员或菜单 Id 无效时抛出。
    /// </exception>
    Task GrantMenusAsync(long id, IReadOnlyCollection<long> menuIds, CancellationToken ct = default);
}
